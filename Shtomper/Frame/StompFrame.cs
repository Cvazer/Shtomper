using Shtomper.Client.Enum;
using Shtomper.Frame.Enum;

namespace Shtomper.Frame;

using static EnumUtils;
using static Shtomper.Frame.Enum.Command;

public record StompFrame
{
    private static readonly HashSet<Command> CommandsWithBody = new() { Send, Command.Message, Error };
    
    internal readonly FrameData Data;

    protected StompFrame(FrameData data)
    {
        Data = data;
        CheckBody();
    }

    protected StompFrame(string data) : this(FrameData.FromString(data)) => CheckBody();

    protected StompFrame(Command command, Dictionary<string, string>? headers = null, string? body = null)
    {
        Data = new FrameData(command, headers, body);
        CheckBody();
    }
    
    private void CheckBody()
    {
        if (!string.IsNullOrEmpty(Data.Body) && !CommandsWithBody.Contains(Data.Command))
        {
            throw new ArgumentException("Only the SEND, MESSAGE, and ERROR frames can have a body");
        }
    }

    public sealed override string ToString() => Data.Stringify();

    public string? Header(StompHeader header) => Data.Headers
        .Select<(string, string), (string?, string?)>(it => it)
        .Where(it => it.Item1!.Equals(HeaderName(header)))
        .FirstOrDefault((null, null))
        .Item2;
    
    public void Header(StompHeader header, string value)
    {
        var existing = Data.Headers
            .Select<(string, string), (string?, string?)>(it => it)
            .FirstOrDefault(it => it.Item1!.Equals(HeaderName(header)), (null, null));

        if (existing.Item2 == null)
        {
            Data.Headers.Add((HeaderName(header), value));
        }
        else
        {
            existing.Item2 = value;
        }
        
    }

    protected string? Destination() => Header(StompHeader.Destination);
    protected void Destination(string value) => Header(StompHeader.Destination, value);
    
    internal string? Transaction() => Header(StompHeader.Transaction);
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

    protected long? Subscription() => LongValue(Header(StompHeader.Subscription));
    protected void Subscription(long value) => Header(StompHeader.Subscription, value.ToString());
    
    protected long? Id() => LongValue(Header(StompHeader.Id));
    protected void Id(long value) => Header(StompHeader.Id, value.ToString());

    private static long? LongValue(string? value)
    {
        if (value == null) return null;
        return long.Parse(value);
    }
}