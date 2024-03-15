namespace Shtomper.Frame.Impl.Server;

public record Connected : StompFrame
{

    public Connected(FrameData data) : base(data)
    {
    }

    public int NegotiateHeartBeat(int capableHeartBeat)
    {
        var serverHeartbeat = (HeartBeat() ?? "0,0").Split(",").Select(int.Parse).ToArray();
        
        var sy = serverHeartbeat[1];
        var cx = capableHeartBeat < 0 ? 0 : capableHeartBeat;

        if (cx == 0 || sy == 0) return 0;

        return Math.Max(cx, sy);
    }
    
    public new string? Version() => base.Version();
    public new string? HeartBeat() => base.HeartBeat();
    
    public new string? Session() => base.Session();
}