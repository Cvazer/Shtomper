using Shtomper.Frame;
using Shtomper.Frame.Impl.Client;
using Shtomper.Frame.Impl.Server;

namespace Shtomper.Client;

public abstract class AbstractStompClientV10 : IStompClient, IDisposable
{
    public delegate void ErrorHandler(Error error);

    public event ErrorHandler? ErrorHandlerEvent;
    
    protected readonly IMessageConverter MessageConverter;
    protected readonly IHeartbeatHandler HeartbeatHandler;
    protected readonly AckMode AckMode;
    protected readonly bool ReceiptMode;

    private readonly Dictionary<string, Action<Message>> _handlers = new();

    private readonly Dictionary<string, ManualResetEventSlim> _receiptLocks = new();

    protected AbstractStompClientV10(
        IMessageConverter messageConverter,
        IHeartbeatHandler heartbeatHandler,
        AckMode ackMode = AckMode.Auto,
        bool receiptMode = false
    )
    {
        MessageConverter = messageConverter;
        HeartbeatHandler = heartbeatHandler;
        AckMode = ackMode;
        ReceiptMode = receiptMode;

        HeartbeatHandler.Start(SendHeartBeat);
    }

    public void Send<T>(string destination, T data)
    {
        Send(destination, data, null);
    }

    internal void Send<T>(string destination, T data, string? txId)
    {
        var frame = MessageConverter.Convert(destination, data);
        if (txId != null) frame.Transaction(txId);
        SendWithAndMaybeReceipt(frame);
    }

    public long Subscribe<T>(string destination, Action<T> callback)
    {
        _handlers[destination] = msg =>
        {
            try
            {
                var obj = MessageConverter.Convert<T>(msg);
                if (obj != null) callback.Invoke(obj);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        };

        var sub = new Subscribe(destination, AckMode);
        SendWithAndMaybeReceipt(sub);

        return sub.Id()!.Value;
    }

    public void Unsubscribe(string destination, long? subId = null)
    {
        SendWithAndMaybeReceipt(new Unsubscribe(destination, subId));
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
                HandleServerMessage(new Message(data));

                break;
            case Command.Receipt:
                HandleReceipt(new Receipt(data));

                break;
            default:
                throw new Exception("Invalid server message");
        }
    }

    private void HandleReceipt(Receipt receipt)
    {
        var guid = receipt.ReceiptId();
        var eventHandle = _receiptLocks!.GetValueOrDefault(guid, null);
        eventHandle?.Set();
    }

    private void HandleServerMessage(Message msg)
    {
        var destination = msg.Destination();
        if (!_handlers.ContainsKey(destination)) return;
        _handlers[destination].Invoke(msg);
        if (AckMode == AckMode.Client)
        {
            SendFrame(new Ack(msg.MessageId()));
        }
    }

    private void HandleServerError(Error error)
    {
        ErrorHandlerEvent?.Invoke(error);
    }

    public virtual void Dispose()
    {
        HeartbeatHandler.Dispose();
    }
    
    private void SendWithAndMaybeReceipt(StompFrame frame)
    {
        if (!ReceiptMode)
        {
            SendFrame(frame);
            return;
        }
        
        var guid = Guid.NewGuid().ToString();
        var eventHandle = new ManualResetEventSlim(false);

        _receiptLocks[guid] = eventHandle;
        frame.Receipt(guid);

        try
        {
            SendFrame(frame);
        }
        catch (Exception)
        {
            _receiptLocks.Remove(guid);
            throw;
        }
        
        eventHandle.Wait(TimeSpan.FromSeconds(30));
        _receiptLocks.Remove(guid);
        
        if (!eventHandle.IsSet)
        {
            throw new StompException($"No receipt arrived [{guid}]");
        }
    }

    protected abstract void SendFrame(StompFrame frame);
    protected abstract void SendHeartBeat();
}