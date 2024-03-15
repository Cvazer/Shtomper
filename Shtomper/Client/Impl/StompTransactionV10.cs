using System.Transactions;
using Shtomper.Frame.Impl.Client;
using TransactionStatus = Shtomper.Client.Enum.TransactionStatus;

namespace Shtomper.Client.Impl;

public class StompTransactionV10 : IStompTransaction
{
    internal delegate void FinalizeHandler();
    internal FinalizeHandler? FinalizedEvent;
    
    private readonly AbstractStompClientV10 _client;
    
    public string TransactionName { get; private set; }
    public TransactionStatus Status { get; private set; }

    public StompTransactionV10(AbstractStompClientV10 client) => _client = client;

    public IStompTransaction Begin(string? txId = null)
    {
        if (Status != TransactionStatus.Created)
        {
            throw new TransactionException("Already started");
        }

        TransactionName = txId ?? Guid.NewGuid().ToString();
        _client.SendMaybeWithReceipt(new Begin(TransactionName));
        Status = TransactionStatus.Ongoing;

        return this;
    }

    public void Commit()
    {
        CheckOngoingTransactionStatus();
        _client.SendMaybeWithReceipt(new Commit(TransactionName));
        Status = TransactionStatus.Closed;
        FinalizedEvent?.Invoke();
    }

    public void Abort()
    {
        CheckOngoingTransactionStatus();
        _client.SendMaybeWithReceipt(new Abort(TransactionName!));
        Status = TransactionStatus.Closed;
        FinalizedEvent?.Invoke();
    }

    public IStompTransaction Send<T>(string destination, T data)
    {
        _client.Send(destination, data, TransactionName);

        return this;
    }

    private void CheckOngoingTransactionStatus()
    {
        if (Status != TransactionStatus.Ongoing)
        {
            throw new TransactionException(
                Status switch
                {
                    TransactionStatus.Closed => "Already closed",
                    TransactionStatus.Created => "Not started yet"
                }
            );
        }
    }
}