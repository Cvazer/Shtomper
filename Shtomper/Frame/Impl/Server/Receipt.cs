namespace Shtomper.Frame.Impl.Server;

public record Receipt : StompFrame
{
    public Receipt(FrameData data) : base(data)
    {
    }
    
    public new string ReceiptId() => base.ReceiptId()!;
}