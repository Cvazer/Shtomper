namespace Shtomper;

public interface IStompClient
{
    void Send<T>(string destination, T data);
    long Subscribe<T>(string destination, Action<T> callback);
    void Unsubscribe(string destination, long? subId = null);
}