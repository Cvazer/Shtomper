using System.Text;

namespace Shtomper.Frame.Impl.Client;

public record Send : StompFrame
{
    public Send(
        string destination,
        string? body,
        ContentType type = ContentType.TextPlain,
        Dictionary<string, string>? userDefinedHeaders = null
    ) : base(Command.Send, userDefinedHeaders, body)
    {
        Header(StompHeader.Destination, destination);

        if (body == null) return;

        Header(StompHeader.ContentType, Utils.ContentTypeValue(type));
        Header(StompHeader.ContentLength, Encoding.ASCII.GetBytes(body).Length.ToString());
    }
    
    public new string? Receipt() => base.Receipt();
    public new void Receipt(string value) => base.Receipt(value);

    public new string? Destination() => base.Destination();
    public new void Destination(string value) => base.Destination(value);
    
    public new string? Transaction() => base.Transaction();
    public new void Transaction(string value) => base.Transaction(value);
}