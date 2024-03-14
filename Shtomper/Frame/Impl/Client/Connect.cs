namespace Shtomper.Frame.Impl.Client;

using static Utils;
using static StompHeader;

public record Connect : StompFrame
{
    public const int DefaultCapableHeartBeat = 0;
    public const int DefaultDesiredHeartBeat = 0;
    
    public Connect(
        string? hostname,
        string? login = null,
        string? passcode = null,
        int heartBeatCapable = DefaultCapableHeartBeat,
        int heartBeatDesired = DefaultDesiredHeartBeat
    ) : base(Command.Connect)
    {
        Data.Headers[HeaderName(AcceptVersion)] = GetCapableVersions(Enum.GetValues<StompVersion>());

        if (hostname != null)
        {
            Data.Headers[HeaderName(Host)] = hostname;
        }

        if (login != null)
        {
            Data.Headers[HeaderName(Login)] = login;
        }

        if (passcode != null)
        {
            Data.Headers[HeaderName(Passcode)] = passcode;
        }

        Data.Headers[HeaderName(StompHeader.HeartBeat)] = GetHeartBeatValue(heartBeatCapable, heartBeatDesired);
    }

    private static string GetCapableVersions(StompVersion[] versions) => versions
        .ToList()
        .Select(StompVersionName)
        .Aggregate((v1, v2) => v1 + "," + v2);
    
    private static string GetHeartBeatValue(int capable, int desired) => $"{capable},{desired}";
}