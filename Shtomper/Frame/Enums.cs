namespace Shtomper.Frame;

public enum AckMode
{
    Client,
    Auto
}

public enum StompVersion
{
    V10 = 0,
    // V11 = 1,
    // V12 = 2
}

public enum StompHeader
{
    Host,
    AcceptVersion,
    Login,
    Passcode,
    HeartBeat,
    Version,
    Session,
    Server,
    Message,
    Destination,
    ContentType,
    ContentLength,
    Transaction,
    Ack,
    Selector,
    Id,
    MessageId,
    Receipt,
    ReceiptId,
}

public enum Command
{
    Send,
    Subscribe,
    Unsubscribe,
    Begin,
    Commit,
    Abort,
    Ack,
    Nack,
    Disconnect,
    Connect,
    Stomp,
    Connected,
    Message,
    Receipt,
    Error
}

public enum ContentType
{
    ApplicationJson,
    TextPlain
}

public static class Utils
{
    private static readonly List<(StompVersion, string)> StompVersionStrings = new()
    {
        ( StompVersion.V10, "1.0" ),
        // ( StompVersion.V11, "1.1" ),
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
    };
    
    private static readonly Dictionary<ContentType, string> ContentTypeStrings = new()
    {
        { ContentType.ApplicationJson, "application/json" },
        { ContentType.TextPlain, "text/plain" }
    };
    
    private static readonly Dictionary<AckMode, string> AckModeStrings = new()
    {
        { AckMode.Client, "client" },
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