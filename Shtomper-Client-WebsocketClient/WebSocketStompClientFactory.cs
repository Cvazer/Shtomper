using System.Net.WebSockets;
using NLog;
using Shtomper.Client;
using Shtomper.Client.Builder;
using Shtomper.Client.Enum;
using Shtomper.Client.Impl;
using Shtomper.Frame;
using Shtomper.Frame.Enum;
using Shtomper.Frame.Impl.Client;
using Shtomper.Frame.Impl.Server;
using Websocket.Client;
using static Shtomper.Frame.EnumUtils;

namespace Shtomper_Client_WebsocketClient;

public class
    WebSocketStompClientFactory : IStompClientFactory<IStompClient, WebSocketStompClientFactoryBuilder>
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly WebSocketStompClientFactoryBuilder _builder;

    public WebSocketStompClientFactory(WebSocketStompClientFactoryBuilder builder) => _builder = builder;

    public IStompClient Create()
    {
        var uriBuilder = new UriBuilder(
            _builder.Schema,
            _builder.Host,
            _builder.Port,
            _builder.Path,
            _builder.Params
        );

        var wsClient = new WebsocketClient(uriBuilder.Uri);

        if (_builder.ReconnectTimout > 0)
        {
            wsClient.ReconnectTimeout = TimeSpan.FromMilliseconds(_builder.ReconnectTimout);
        }

        wsClient.Start().Wait(TimeSpan.FromSeconds(30));

        if (!wsClient.IsRunning)
        {
            throw new WebSocketException("Connection Attempt timeout");
        }

        var connectedEvent = new ManualResetEvent(false);
        Connected? connectedFrame = null;
        Exception? connectException = null;
        wsClient.MessageReceived.Subscribe(
            msg =>
            {
                if (msg.Text == null) return;

                FrameData data;
                try
                {
                    data = FrameData.FromString(msg.Text!);
                }
                catch (ArgumentException)
                {
                    return;
                }

                if (data.Command == Command.Error)
                {
                    connectException = new StompException(data.Body);
                    connectedEvent.Set();
                    return;
                }

                if (data.Command != Command.Connected)
                {
                    connectedEvent.Set();
                    return;
                }

                connectedFrame = new Connected(data);
                connectedEvent.Set();
            }
        );

        var connectFrame = new Connect(
            _builder.HostOverride ?? _builder.Host,
            _builder.ClientBuilder!.Username,
            _builder.ClientBuilder!.Passcode,
            _builder.ClientBuilder!.HeartbeatCapable,
            _builder.ClientBuilder!.HeartbeatDesired
        );

        Logger.Trace("\n" + connectFrame);
        wsClient.Send(connectFrame.ToString());

        connectedEvent.WaitOne(TimeSpan.FromSeconds(30));

        if (connectException != null)
        {
            throw connectException;
        }

        if (connectedFrame == null)
        {
            throw new InvalidDataException("No CONNECTED frame received");
        }

        Logger.Trace("\n" + connectedFrame);

        var heartbeatHandler = CreateHeartbeatHandler(connectedFrame!);
        var stompClient = CreateVersionedClient(heartbeatHandler, wsClient, connectedFrame);

        stompClient.Start();
        return stompClient;
    }

    private IWebSocketStompClient CreateVersionedClient(
        IHeartbeatHandler heartbeatHandler,
        WebsocketClient wsClient,
        Connected connectedFrame
    )
    {
        var version = ParseVersion(connectedFrame.Version() ?? "1.0");
        return version switch
        {
            StompVersion.V10 => new WebSocketStompClientV10(
                _builder.ClientBuilder!.MessageConverter!,
                heartbeatHandler,
                wsClient,
                _builder.ClientBuilder!.ReceiptMode,
                _builder.ClientBuilder!.NackMode,
                _builder.ClientBuilder.DebugHeartbeat
            ),
            StompVersion.V11 => new WebSocketStompClientV11(
                _builder.ClientBuilder!.MessageConverter!,
                heartbeatHandler,
                wsClient,
                _builder.ClientBuilder!.ReceiptMode,
                _builder.ClientBuilder!.NackMode,
                _builder.ClientBuilder.DebugHeartbeat
            ),
            StompVersion.V12 => new WebSocketStompClientV12(
                _builder.ClientBuilder!.MessageConverter!,
                heartbeatHandler,
                wsClient,
                _builder.ClientBuilder!.ReceiptMode,
                _builder.ClientBuilder!.NackMode,
                _builder.ClientBuilder.DebugHeartbeat
            ),
            _ => throw new ArgumentException("Invalid version")
        };
    }

    private IHeartbeatHandler CreateHeartbeatHandler(Connected connectedFrame)
    {
        var version = connectedFrame.Version() ?? StompVersionName(StompVersion.V10);
        if (version.Equals(StompVersionName(StompVersion.V10))) return new NoOpHeartbeatHandler();

        var hb = connectedFrame.NegotiateHeartBeat(_builder.ClientBuilder!.HeartbeatCapable);
        if (hb == 0) return new NoOpHeartbeatHandler();

        return new DefaultHeartbeatHandler(hb);
    }
}