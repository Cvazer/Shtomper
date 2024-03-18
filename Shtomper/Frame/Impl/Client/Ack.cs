using Shtomper.Frame.Enum;

namespace Shtomper.Frame.Impl.Client;

public record Ack : StompFrame 
{

    public Ack(string? txId = null) : this(Command.Ack, txId) { }

    protected Ack(Command command, string? txId = null) : base(command)
    {
        if (txId != null) Transaction(txId);
    }

    public new string? Receipt() => base.Receipt();
    public new void Receipt(string value) => base.Receipt(value);
    
    public new string MessageId() => base.MessageId()!;
    public new void MessageId(string value) => Header(StompHeader.MessageId, value);
    
    public new string? Transaction() => base.Transaction();
    public new void Transaction(string value) => base.Transaction(value);

    public new void Subscription(long value) => base.Subscription(value);
}