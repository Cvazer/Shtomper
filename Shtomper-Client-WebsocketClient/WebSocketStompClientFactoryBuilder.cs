using Shtomper.Client.Builder;

namespace Shtomper_Client_WebsocketClient;

public class WebSocketStompClientFactoryBuilder : IStompClientFactoryBuilder<WebSocketStompClientFactory>
{
    internal int ReconnectTimout;
    internal string Schema = "ws";
    internal string Host = "localhost";
    internal string? HostOverride;
    internal int Port = 3000;
    internal string Path = "ws";
    internal string Params = "";

    public StompClientBuilder? ClientBuilder { get; set; }
    

    private WebSocketStompClientFactoryBuilder SetParams(params string[] queryParams)
    {
        Params = queryParams.Aggregate((s1, s2) => $"{s1}&{s2}");
        return this;
    }

    public WebSocketStompClientFactoryBuilder SetReconnectTimout(int millis)
    {
        ReconnectTimout = millis;
        return this;
    }

    public WebSocketStompClientFactoryBuilder SetSecure(bool secure)
    {
        Schema = secure ? "wss" : "ws";
        return this;
    }

    public WebSocketStompClientFactoryBuilder SetHost(string host)
    {
        Host = host;
        return this;
    }
    
    public WebSocketStompClientFactoryBuilder SetHostOverride(string hostOverride)
    {
        HostOverride = hostOverride;
        return this;
    }

    public WebSocketStompClientFactoryBuilder SetPort(int port)
    {
        Port = port;
        return this;
    }

    public WebSocketStompClientFactoryBuilder SetPath(string path)
    {
        Path = path;
        return this;
    }

    public WebSocketStompClientFactory Build() => new(this);
}