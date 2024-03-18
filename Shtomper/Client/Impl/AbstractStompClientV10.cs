using System.Collections.Concurrent;
using Shtomper.Client.Enum;
using Shtomper.Frame;
using Shtomper.Frame.Enum;
using Shtomper.Frame.Impl.Client;
using Shtomper.Frame.Impl.Server;

namespace Shtomper.Client.Impl;

public abstract class AbstractStompClientV10 : IStompClient
{
    private static readonly HashSet<Command> DoReceiptOnTransaction = new()
    {
        Command.Commit,
        Command.Begin,
        Command.Abort
    };

    public event IStompClient.ErrorHandler? ErrorHandlerEvent;
    public event IStompClient.MessageDiscardedHandler? MessageDiscardedEvent;

    public IMessageConverter MessageConverter { get; }
    public IHeartbeatHandler HeartbeatHandler { get; }
    public bool ReceiptMode { get; }
    public bool NackMode { get; }

    protected readonly ConcurrentDictionary<string, ConcurrentDictionary<long, Action<Message>>> Handlers = new();
    protected readonly ConcurrentDictionary<string, ManualResetEventSlim> ReceiptLocks = new();
    protected readonly ConcurrentDictionary<string, ConcurrentDictionary<long, AckMode>> SubInfo = new();
    protected readonly ConcurrentDictionary<string, IStompTransaction> Transactions = new();

    protected readonly ConcurrentQueue<Message> MessageQueue = new();
    protected readonly ReaderWriterLock MessageQueueLock = new();
    protected readonly AutoResetEvent MessageQueueWriteEvent = new(false);
    protected readonly CancellationTokenSource StopToken = new();

    private bool _disposed;

    protected AbstractStompClientV10(
        IMessageConverter messageConverter,
        IHeartbeatHandler heartbeatHandler,
        bool receiptMode = false,
        bool nackMode = false
    )
    {
        MessageConverter = messageConverter;
        HeartbeatHandler = heartbeatHandler;
        NackMode = nackMode;
        ReceiptMode = receiptMode;

        ThreadPool.QueueUserWorkItem(_ => HeartbeatHandler.Start(SendHeartBeat));
    }

    protected void Start()
    {
        DoMessageProcessing(StopToken.Token);
    }

    private async void DoMessageProcessing(CancellationToken stopTokenToken)
    {
        await Task.Run(
            () =>
            {
                while (!stopTokenToken.IsCancellationRequested)
                {
                    try
                    {
                        MessageQueueLock.AcquireReaderLock(-1);
                        Message? lastMsg = null;
                        foreach (var message in MessageQueue)
                        {
                            ThreadPool.QueueUserWorkItem(_ => HandleServerMessage(message));
                            lastMsg = message;
                        }

                        DoPostMsgHandle(lastMsg);
                    }
                    finally
                    {
                        MessageQueueLock.ReleaseReaderLock();
                    }

                    MessageQueueWriteEvent.WaitOne();
                }
            },
            stopTokenToken
        );
    }

    protected virtual void DoPostMsgHandle(Message? lastMsg)
    {
        if (lastMsg == null) return;
        MessageQueue.Clear();
    }

    public void Send<T>(string destination, T data, Dictionary<string, string>? userHeaders = null)
    {
        Send(destination, data, txId: null);
    }

    internal void Send<T>(string destination, T data, string? txId, Dictionary<string, string>? userHeaders = null)
    {
        var frame = MessageConverter.Convert(destination, data, userHeaders);
        if (txId != null) frame.Transaction(txId);
        SendMaybeWithReceipt(frame);
    }

    public long Subscribe<T>(string destination, Action<T> callback, AckMode ackMode = AckMode.Auto)
    {
        var sub = new Subscribe(destination, ackMode);

        if (!Handlers.ContainsKey(destination))
        {
            Handlers[destination] = new ConcurrentDictionary<long, Action<Message>>();
        }

        if (!SubInfo.ContainsKey(destination))
        {
            SubInfo[destination] = new ConcurrentDictionary<long, AckMode>();
        }

        void Handler(Message msg)
        {
            var obj = MessageConverter.Convert<T>(msg);
            if (obj != null) callback.Invoke(obj);
        }

        Handlers[destination][-1] = Handler;
        Handlers[destination][sub.Id()!.Value] = Handler;

        SubInfo[destination][-1] = ackMode;
        SubInfo[destination][sub.Id()!.Value] = ackMode;

        SendMaybeWithReceipt(sub);

        return sub.Id()!.Value;
    }

    public IStompTransaction Transaction(string? txId = null)
    {
        var tx = new StompTransactionV10(this);
        var transactionName = tx.Begin(txId).TransactionName;
        Transactions[transactionName!] = tx;
        tx.FinalizedEvent += () => Transactions!.Remove(transactionName, out _);

        return tx;
    }

    public void Unsubscribe(string destination, long? subId = null)
    {
        if (!Handlers.ContainsKey(destination)) return;
        Handlers.Remove(destination, out _);
        SubInfo.Remove(destination, out _);
        SendMaybeWithReceipt(new Unsubscribe(destination, subId));
    }

    protected void HandleMessage(string msg)
    {
        var data = FrameData.FromString(msg);

        switch (data.Command)
        {
            case Command.Error:
                HandleServerError(new Error(data));

                break;
            case Command.Message:
                try
                {
                    MessageQueueLock.AcquireWriterLock(-1);
                    MessageQueue.Enqueue(new Message(data));
                    MessageQueueWriteEvent.Set();
                }
                finally
                {
                    MessageQueueLock.ReleaseWriterLock();
                }

                break;
            case Command.Receipt:
                HandleReceiptFrame(new Receipt(data));

                break;
            default:
                throw new Exception("Invalid server message");
        }
    }

    private void HandleReceiptFrame(Receipt receipt) => HandleReceipt(receipt.ReceiptId());

    private void HandleReceipt(string receipt)
    {
        var eventHandle = ReceiptLocks!.GetValueOrDefault(receipt, null);
        eventHandle?.Set();
    }

    private void HandleServerMessage(Message msg)
    {
        var destination = msg.Destination();
        var subId = msg.Subscription() ?? -1;
        var txId = msg.Transaction();
        var ackMode = SubInfo[destination][subId];

        if (!Handlers.ContainsKey(destination))
        {
            if (ackMode != AckMode.Auto) DoNack(msg, subId, txId);
            MessageDiscardedEvent?.Invoke(msg);
            return;
        }

        try
        {
            Handlers[destination][subId].Invoke(msg);
        }
        catch (Exception)
        {
            if (ackMode != AckMode.Auto) DoNack(msg, subId, txId);
            return;
        }

        if (ackMode != AckMode.Auto) AckMessage(ackMode, msg, txId, subId);
    }

    private void HandleServerError(Error error)
    {
        var receipt = error.Receipt();
        if (receipt != null) HandleReceipt(receipt);

        if (!CheckConnection()) Dispose();

        ErrorHandlerEvent?.Invoke(error);
    }

    internal void SendMaybeWithReceipt(StompFrame frame)
    {
        if (_disposed) throw new StompException("Already disposed");

        var doReceiptIfTransaction = frame.Transaction() != null
                                     && !DoReceiptOnTransaction.Contains(frame.Data.Command);

        if (!ReceiptMode || doReceiptIfTransaction || frame.Data.Command == Command.Disconnect)
        {
            SendFrame(frame);

            return;
        }

        var guid = Guid.NewGuid().ToString();
        var eventHandle = new ManualResetEventSlim(false);

        ReceiptLocks[guid] = eventHandle;
        frame.Receipt(guid);

        try
        {
            SendFrame(frame);
        }
        catch (Exception)
        {
            ReceiptLocks.Remove(guid, out _);

            throw;
        }

        eventHandle.Wait(TimeSpan.FromSeconds(30));
        ReceiptLocks.Remove(guid, out _);

        if (!eventHandle.IsSet)
        {
            throw new StompException($"No receipt arrived [{guid}]");
        }
    }

    protected virtual void AckMessage(AckMode ackMode, Message msg, string? txId, long subId)
    {
        if (ackMode != AckMode.Client) return;
        DoActualAck(msg, txId, subId);
    }

    protected void DoActualAck(Message msg, string? txId, long subId) =>
        DoActualAck(new Ack(), msg, txId, subId);

    protected virtual void DoActualAck(Ack obj, Message msg, string? txId, long subId)
    {
        obj.MessageId(msg.MessageId());
        if (txId != null) obj.Transaction(txId);
        SendMaybeWithReceipt(obj);
    }

    private void DoNack(Message msg, long subId, string? txId)
    {
        if (!NackMode) return;
        DoActualAck(new Nack(), msg, txId, subId);
    }

    public virtual void Dispose()
    {
        if (_disposed) return;

        SendMaybeWithReceipt(new Disconnect());

        StopToken.Cancel();
        MessageQueueWriteEvent.Set();

        foreach (var eventLock in ReceiptLocks.Values)
        {
            eventLock.Set();
            eventLock.Dispose();
        }

        HeartbeatHandler.Dispose();
        MessageQueueWriteEvent.Dispose();
        StopToken.Dispose();

        _disposed = true;
    }

    protected abstract void SendFrame(StompFrame frame);
    protected abstract void SendHeartBeat();
    protected abstract bool CheckConnection();
}