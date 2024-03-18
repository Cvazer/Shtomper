using Shtomper.Client;
using Shtomper.Client.Impl;
using Shtomper.Frame;
using Websocket.Client;

namespace Shtomper_Client_WebsocketClient;

public class WebSocketStompClientV12 : AbstractStompClientV12, IWebSocketStompClient
{
    private readonly WebSocketStompClientCommonHandler _handler;
    
    public WebSocketStompClientV12(
        IMessageConverter messageConverter,
        IHeartbeatHandler heartbeatHandler,
        WebsocketClient websocketClient,
        bool receiptMode = false,
        bool nackMode = false,
        bool debugHb = false
    ) : base(
        messageConverter,
        heartbeatHandler,
        receiptMode,
        nackMode
    ) => _handler = new WebSocketStompClientCommonHandler(this, websocketClient, heartbeatHandler, debugHb);

    public new void Start()
    {
        _handler.Start();
        base.Start();
    }

    public void Handle(string message) => HandleMessage(message);

    protected override void SendFrame(StompFrame frame) => _handler.SendFrame(frame);

    protected override void SendHeartBeat() => _handler.SendHeartBeat();
    
    protected override bool CheckConnection() => _handler.WebsocketClient.IsRunning;
    
    public override void Dispose()
    {
        base.Dispose();
        _handler.Dispose();
    }
}