namespace Shtomper.Frame.Impl.Server;

public record Message : StompFrame
{
    public Message(FrameData data) : base(data) 
    {
        if (Header(StompHeader.Destination) == null)
        {
            throw new ArgumentException("Message doesn't specify a destination");
        }
    }

    public string? Body() => Data.Body;

    public new string Destination() => base.Destination()!;
    public new string MessageId() => base.MessageId()!;
};