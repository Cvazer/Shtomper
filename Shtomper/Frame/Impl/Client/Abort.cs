namespace Shtomper.Frame.Impl.Client;

public record Abort : StompFrame
{
    public Abort(string txId) : base(Command.Abort)
    {
        Transaction(txId);
    }
    
    public new string? Receipt() => base.Receipt();
    public new void Receipt(string value) => base.Receipt(value);
    
    public new string Transaction() => base.Transaction()!;
    public new void Transaction(string value) => base.Transaction(value);
}