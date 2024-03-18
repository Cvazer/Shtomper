using Shtomper.Client.Enum;
using Shtomper.Frame;
using Shtomper.Frame.Impl.Client;
using Shtomper.Frame.Impl.Server;
using Websocket.Client;

namespace Shtomper.Client.Builder;

public class StompClientBuilder
{
    public IMessageConverter? MessageConverter { get; private set; }

    public string Username { get; private set; } = "guest";
    public string Passcode { get; private set; } = "guest";
    public int HeartbeatDesired { get; private set; }
    public int HeartbeatCapable { get; private set; }
    public bool NackMode { get; private set; } = true;
    public bool ReceiptMode { get; private set; }
    public bool DebugHeartbeat { get; private set; }

    public StompClientBuilder SetHeartbeatDesired(int heartbeatDesired)
    {
        HeartbeatDesired = heartbeatDesired;
        return this;
    }

    public StompClientBuilder SetHeartbeatCapable(int heartbeatCapable)
    {
        HeartbeatCapable = heartbeatCapable;
        return this;
    }

    public StompClientBuilder SetUsername(string username)
    {
        Username = username;
        return this;
    }

    public StompClientBuilder SetPasscode(string passcode)
    {
        Passcode = passcode;
        return this;
    }

    public StompClientBuilder SetMessageConverter(IMessageConverter converter)
    {
        MessageConverter = converter;
        return this;
    }

    public StompClientBuilder SetDebugHeartbeat(bool debugHeartbeat)
    {
        DebugHeartbeat = debugHeartbeat;
        return this;
    }

    public StompClientBuilder SetNackMode(bool nackMode)
    {
        NackMode = nackMode;
        return this;
    }

    public TFactoryBuilder WithBuilder<TFactoryBuilder>(TFactoryBuilder builderInstance)
        where TFactoryBuilder : IStompClientFactoryBuilder<IStompClientFactory<IStompClient, TFactoryBuilder>>
    {
        CheckMessageConverter();
        builderInstance.ClientBuilder = this;
        return builderInstance;
    }

    private void CheckMessageConverter()
    {
        if (MessageConverter == null)
        {
            throw new InvalidDataException("Message converter must be specified");
        }
    }
}