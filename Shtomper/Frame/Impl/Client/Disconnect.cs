namespace Shtomper.Frame.Impl.Client;

public record Disconnect : StompFrame 
{
    protected Disconnect(string data) : base(data)
    {
    }
    
    public new string? Receipt() => base.Receipt();
    public new void Receipt(string value) => base.Receipt(value);
}