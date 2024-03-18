using NLog;
using Shtomper.Client;
using Shtomper.Frame;
using Websocket.Client;

namespace Shtomper_Client_WebsocketClient;

public class WebSocketStompClientCommonHandler : IDisposable
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    internal WebsocketClient WebsocketClient { get; }
    
    private readonly IWebSocketStompClient _stompClient;
    private readonly IHeartbeatHandler _heartbeatHandler;
    private readonly bool _debugHb;

    public WebSocketStompClientCommonHandler(
        IWebSocketStompClient stompClient,
        WebsocketClient websocketClient,
        IHeartbeatHandler heartbeatHandler,
        bool debugHb = false
    ) => (_stompClient, WebsocketClient, _heartbeatHandler, _debugHb) =
        (stompClient, websocketClient, heartbeatHandler, debugHb);

    internal void Start()
    {
        WebsocketClient.MessageReceived.Subscribe(OnMessage);
        _heartbeatHandler.Start(SendHeartBeat);
    }

    internal void SendFrame(StompFrame frame)
    {
        if (!WebsocketClient.IsRunning)
        {
            throw new StompException("Transport connection Lost");
        }

        var data = frame.ToString();
        Logger.Trace("\n" + data);
        WebsocketClient.Send(data);
    }

    internal void SendHeartBeat()
    {
        if (_debugHb) Logger.Trace("HB >>");
        WebsocketClient.Send("" + char.MinValue);
    }

    private void OnMessage(ResponseMessage msg)
    {
        if (msg.Text is null or "\n")
        {
            if (_debugHb) Logger.Trace("HB <<");

            return; //Due to being a heartbeat (probably)
        }

        Logger.Trace("\n" + msg.Text);
        _stompClient.Handle(msg.Text);
    }

    public void Dispose()
    {
        WebsocketClient.Dispose();
    }
}