using Shtomper.Client;
using Shtomper.Frame;
using Websocket.Client;

namespace Shtomper_Client_WebsocketClient;

public class WebSocketStompClientV10 : AbstractStompClientV10
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    
    private readonly WebsocketClient _websocketClient;

    public WebSocketStompClientV10(
        IMessageConverter messageConverter,
        IHeartbeatHandler heartbeatHandler,
        WebsocketClient websocketClient,
        AckMode ackMode,
        bool receiptMode
    ) : base(
        messageConverter,
        heartbeatHandler,
        ackMode,
        receiptMode
    ) => _websocketClient = websocketClient;

    public void Start()
    {
        _websocketClient.MessageReceived.Subscribe(OnMessage);
        HeartbeatHandler.Start(SendHeartBeat);
    }

    protected sealed override void SendFrame(StompFrame frame)
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
        _websocketClient.Send("" + char.MinValue);
    }

    private void OnMessage(ResponseMessage msg)
    {
        if (msg.Text is null or "\n") return; //Due to being a heartbeat (probably)
        Logger.Trace("\n" + msg.Text);
        HandleMessage(msg.Text);
    }

    public override void Dispose()
    {
        base.Dispose();
        _websocketClient.Dispose();
    }
}