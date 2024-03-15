using Shtomper.Frame.Enum;

namespace Shtomper.Frame.Impl.Client;

public record Disconnect() : StompFrame(Command.Disconnect)
{

    public new string? Receipt() => base.Receipt();
    public new void Receipt(string value) => base.Receipt(value);
}