using Shtomper.Frame.Enum;

namespace Shtomper.Frame.Impl.Server;

public record Error : StompFrame
{
    public Error(FrameData data) : base(data)
    {
    }

    public new string Message() => base.Message()!;
    public new string? Receipt() => base.Receipt();
}