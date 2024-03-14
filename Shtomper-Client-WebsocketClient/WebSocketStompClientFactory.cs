using NLog;
using Shtomper.Client;
using Shtomper.Client.Builder;
using Shtomper.Client.Impl;
using Shtomper.Frame;
using Shtomper.Frame.Impl.Client;
using Shtomper.Frame.Impl.Server;
using Websocket.Client;

namespace Shtomper_Client_WebsocketClient;

public class WebSocketStompClientFactory : IStompClientFactory<WebSocketStompClientV10, WebSocketStompClientFactoryBuilder>
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public WebSocketStompClientFactory(WebSocketStompClientFactoryBuilder builder)
    {
        Builder = builder;
    }

    public WebSocketStompClientFactoryBuilder Builder { get; set; }

    public WebSocketStompClientV10 Create()
    {
        var uriBuilder = new UriBuilder(
            Builder.Schema,
            Builder.Host,
            Builder.Port,
            Builder.Path,
            Builder.Params
        );

        var wsClient = new WebsocketClient(uriBuilder.Uri);

        if (Builder.ReconnectTimout > 0)
        {
            wsClient.ReconnectTimeout = TimeSpan.FromMilliseconds(Builder.ReconnectTimout);
        }

        wsClient.Start().Wait();

        var connectedEvent = new ManualResetEvent(false);
        Connected? connectedFrame = null;
        Exception? connectException = null;
        wsClient.MessageReceived.Subscribe(
            msg =>
            {
                if (msg.Text == null)
                {
                    return;
                }

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
            Builder.HostOverride ?? Builder.Host,
            Builder.ClientBuilder!.Username,
            Builder.ClientBuilder!.Passcode,
            Builder.ClientBuilder!.HeartbeatCapable,
            Builder.ClientBuilder!.HeartbeatDesired
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
        var stompClient = new WebSocketStompClientV10(
            Builder.ClientBuilder.MessageConverter!,
            heartbeatHandler,
            wsClient,
            Builder.ClientBuilder.AckMode,
            Builder.ClientBuilder.ReceiptMode
        );

        stompClient.Start();

        return stompClient;
    }

    private IHeartbeatHandler CreateHeartbeatHandler(Connected connectedFrame)
    {
        var hb = connectedFrame.NegotiateHeartBeat(Builder.ClientBuilder!.HeartbeatCapable);

        if (hb == 0) return new NoOpHeartbeatHandler();

        return new DefaultHeartbeatHandler(hb);
    }
}