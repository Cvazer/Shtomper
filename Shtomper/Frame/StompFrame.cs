namespace Shtomper.Frame;

using static Utils;

public record StompFrame
{
    protected readonly FrameData Data;

    protected StompFrame(FrameData data) => Data = data;

    protected StompFrame(string data) : this(FrameData.FromString(data))
    {
    }

    protected StompFrame(Command command, Dictionary<string, string>? headers = null, string? body = null) =>
        Data = new FrameData(command, headers, body);

    public sealed override string ToString() => Data.Stringify();

    public string? Header(StompHeader header) => Data.Headers!.GetValueOrDefault(HeaderName(header), null);
    public void Header(StompHeader header, string value) => Data.Headers[HeaderName(header)] = value;
    
    protected string? Destination() => Header(StompHeader.Destination);
    protected void Destination(string value) => Header(StompHeader.Destination, value);
    
    protected string? Transaction() => Header(StompHeader.Transaction);
    protected void Transaction(string value) => Header(StompHeader.Transaction, value);
    
    protected string? Session() => Header(StompHeader.Session);
    protected void Session(string value) => Header(StompHeader.Session, value);
    
    protected string? Version() => Header(StompHeader.Version);
    protected string? HeartBeat() => Header(StompHeader.HeartBeat);
    
    protected string? Ack() => Header(StompHeader.Ack);
    protected void Ack(AckMode value) => Header(StompHeader.Ack, AckModeValue(value));
    
    protected string? Selector() => Header(StompHeader.Selector);
    protected void Selector(string value) => Header(StompHeader.Selector, value);
    
    protected string? Message() => Header(StompHeader.Message);
    protected void Message(string value) => Header(StompHeader.Message, value);
    
    protected string? MessageId() => Header(StompHeader.MessageId);
    protected void MessageId(string value) => Header(StompHeader.MessageId, value);
    
    protected string? Receipt() => Header(StompHeader.Receipt);
    internal void Receipt(string value) => Header(StompHeader.Receipt, value);
    
    protected string? ReceiptId() => Header(StompHeader.ReceiptId);
    protected void ReceiptId(string value) => Header(StompHeader.ReceiptId, value);
    
    protected long? Id()
    {
        var value = Header(StompHeader.Id);

        if (value == null) return null;

        return long.Parse(value);
    }
    protected void Id(long value) => Header(StompHeader.Id, value.ToString());
}