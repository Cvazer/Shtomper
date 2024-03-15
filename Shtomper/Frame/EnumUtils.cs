using Shtomper.Client.Enum;
using Shtomper.Frame.Enum;

namespace Shtomper.Frame;

public static class EnumUtils
{
    private static readonly List<(StompVersion, string)> StompVersionStrings = new()
    {
        ( StompVersion.V10, "1.0" ),
        ( StompVersion.V11, "1.1" ),
        // ( StompVersion.V12, "1.2" )
    };

    private static readonly Dictionary<StompHeader, string> StompHeaderStrings = new()
    {
        { StompHeader.Host, "host" },
        { StompHeader.AcceptVersion, "accept-version" },
        { StompHeader.Login, "login" },
        { StompHeader.Passcode, "passcode" },
        { StompHeader.HeartBeat, "heart-beat" },
        { StompHeader.Version, "version" },
        { StompHeader.Session, "session" },
        { StompHeader.Server, "server" },
        { StompHeader.Message, "message" },
        { StompHeader.Destination, "destination" },
        { StompHeader.ContentType, "content-type" },
        { StompHeader.ContentLength, "content-length" },
        { StompHeader.Transaction, "transaction" },
        { StompHeader.Ack, "ack" },
        { StompHeader.Selector, "selector" },
        { StompHeader.Id, "id" },
        { StompHeader.MessageId, "message-id" },
        { StompHeader.Receipt, "receipt" },
        { StompHeader.ReceiptId, "receipt-id" },
        { StompHeader.Subscription, "subscription" },
    };
    
    private static readonly Dictionary<ContentType, string> ContentTypeStrings = new()
    {
        { ContentType.ApplicationJson, "application/json" },
        { ContentType.TextPlain, "text/plain" }
    };
    
    private static readonly Dictionary<AckMode, string> AckModeStrings = new()
    {
        { AckMode.Client, "client" },
        { AckMode.ClientIndividual, "client-individual" },
        { AckMode.Auto, "auto" },
    };

    private static readonly HashSet<Command> ServerCommands = new()
    {
        Command.Connect,
        Command.Error,
        Command.Message,
        Command.Receipt
    };

    public static string HeaderName(StompHeader stompHeader) => StompHeaderStrings[stompHeader];
    public static string ContentTypeValue(ContentType contentType) => ContentTypeStrings[contentType];
    public static string AckModeValue(AckMode ackMode) => AckModeStrings[ackMode];

    public static string StompVersionName(StompVersion stompVersion) => StompVersionStrings
        .Find(it => it.Item1 == stompVersion).Item2;

    public static StompVersion ParseVersion(string stompVersionName) => StompVersionStrings
        .Find(it => it.Item2.Equals(stompVersionName)).Item1;

    public static bool IsServerCommand(Command command) => ServerCommands.Contains(command);

}