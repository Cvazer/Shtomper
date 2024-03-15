using NLog;
using Shtomper.Client;
using Shtomper.Client.Impl;
using Shtomper.Frame;
using Websocket.Client;

namespace Shtomper_Client_WebsocketClient;

public class WebSocketStompClientV11 : AbstractStompClientV11, IWebSocketStompClient
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    
    private readonly WebsocketClient _websocketClient;
    private readonly bool _debugHb;

    public WebSocketStompClientV11(
        IMessageConverter messageConverter,
        IHeartbeatHandler heartbeatHandler,
        WebsocketClient websocketClient,
        bool receiptMode = false,
        bool debugHb = false
    ) : base(
        messageConverter,
        heartbeatHandler,
        receiptMode
    )
    {
        _websocketClient = websocketClient;
        _debugHb = debugHb;
    }

    public new void Start()
    {
        _websocketClient.MessageReceived.Subscribe(OnMessage);
        HeartbeatHandler.Start(SendHeartBeat);
        base.Start();
    }

    protected override void SendFrame(StompFrame frame)
    {
        if (!_websocketClient.IsRunning)
        {
            throw new StompException("Transport connection Lost");
        }
        
        var data = frame.ToString();
        Logger.Trace("\n" + data);
        _websocketClient.Send(data);
    }
    
    protected override void SendHeartBeat()
    {
        if (_debugHb) Logger.Trace("HB >>");
        _websocketClient.Send("" + char.MinValue);
    }
    
    protected override bool CheckConnection() => _websocketClient.IsRunning;

    private void OnMessage(ResponseMessage msg)
    {
        if (msg.Text is null or "\n")
        {
            if (_debugHb) Logger.Trace("HB <<");
            return; //Due to being a heartbeat (probably)
        }
        Logger.Trace("\n" + msg.Text);
        HandleMessage(msg.Text);
    }

    public override void Dispose()
    {
        base.Dispose();
        _websocketClient.Dispose();
    }
}