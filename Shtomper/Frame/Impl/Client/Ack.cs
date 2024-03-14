namespace Shtomper.Frame.Impl.Client;

public record Ack : StompFrame 
{

    public Ack(string msgId, string? txId = null) : base(Command.Ack)
    {
        MessageId(msgId);
        if (txId != null) Transaction(txId);
    }
    
    public new string? Receipt() => base.Receipt();
    public new void Receipt(string value) => base.Receipt(value);
    
    public new string MessageId() => base.MessageId()!;
    public new void MessageId(string value) => Header(StompHeader.MessageId, value);
    
    public new string? Transaction() => base.Transaction();
    public new void Transaction(string value) => base.Transaction(value);
}