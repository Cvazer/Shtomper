using Shtomper.Frame.Enum;

namespace Shtomper.Frame.Impl.Client;

public record Nack : Ack
{

    public Nack(string? txId = null) : base(Command.Nack, txId) { }
}