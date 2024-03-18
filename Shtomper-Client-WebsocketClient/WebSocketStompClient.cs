using Shtomper.Client;

namespace Shtomper_Client_WebsocketClient;

public interface IWebSocketStompClient : IStompClient
{
    public void Start();
    internal void Handle(string message);
}