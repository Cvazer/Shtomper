namespace Shtomper.Frame.Impl.Client;

public record Unsubscribe : StompFrame
{

    public Unsubscribe(string destination, long? subId = null) : base(Command.Unsubscribe)
    {
        Destination(destination);
        if (subId != null) Id(subId.Value);
    }
    
    public new string? Receipt() => base.Receipt();
    public new void Receipt(string value) => base.Receipt(value);
 
    public new string Destination() => base.Destination()!;
    public new void Destination(string value) => base.Destination(value);

    public new long? Id() => base.Id();
    public new void Id(long value) => base.Id(value);
}