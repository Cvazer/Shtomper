using Shtomper.Client.Enum;
using Shtomper.Frame.Impl.Server;

namespace Shtomper.Client;

public interface IStompClient : IDisposable
{
    public delegate void ErrorHandler(Error error);
    public delegate void MessageDiscardedHandler(Message message);
    
    public event ErrorHandler? ErrorHandlerEvent;
    public event MessageDiscardedHandler? MessageDiscardedEvent;
    
    void Send<T>(string destination, T data, Dictionary<string, string>? userDefinedHeaders = null);
    long Subscribe<T>(string destination, Action<T> callback, AckMode ackMode = AckMode.Auto);
    void Unsubscribe(string destination, long? subId = null);
    IStompTransaction Transaction(string? txId = null);
}