namespace Shtomper.Frame.Impl.Client;

public record Subscribe : StompFrame
{
    private static long _subId;

    internal Subscribe(
        string destination,
        AckMode? ackMode = null,
        string? selector = null
    ) : base(Command.Subscribe)
    {
        Destination(destination);

        if (ackMode != null) Ack(ackMode.Value);
        if (selector != null) Selector(selector);

        Id(Interlocked.Increment(ref _subId));
    }
    
    public new string? Receipt() => base.Receipt();
    public new void Receipt(string value) => base.Receipt(value);

    public new string? Ack() => base.Ack();
    public new void Ack(AckMode value) => base.Ack(value);

    public new string? Selector() => base.Selector();
    public new void Selector(string value) => base.Selector(value);
    
    public new string? Destination() => base.Destination();
    public new void Destination(string value) => base.Destination(value);

    public new long? Id() => base.Id();
    public new void Id(long value) => base.Id(value);
}