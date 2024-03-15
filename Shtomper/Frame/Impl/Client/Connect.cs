using Shtomper.Client.Enum;
using Shtomper.Frame.Enum;

namespace Shtomper.Frame.Impl.Client;

using static EnumUtils;
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
        Header(AcceptVersion, GetCapableVersions(System.Enum.GetValues<StompVersion>()));
        Header(StompHeader.HeartBeat, GetHeartBeatValue(heartBeatCapable, heartBeatDesired));

        if (hostname != null) Header(Host, hostname);
        if (login != null) Header(Login, login);
        if (passcode != null) Header(Passcode, passcode);
    }

    private static string GetCapableVersions(StompVersion[] versions)
    {
        return versions
            .ToList()
            .Select(StompVersionName)
            .Aggregate((v1, v2) => v1 + "," + v2);
    }
    
    private static string GetHeartBeatValue(int capable, int desired) => $"{capable},{desired}";
}