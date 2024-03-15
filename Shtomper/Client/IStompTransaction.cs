using Shtomper.Client.Enum;

namespace Shtomper.Client;

public interface IStompTransaction
{
    TransactionStatus Status { get; }
    string TransactionName { get; }
    
    void Commit();
    void Abort();
    IStompTransaction Send<T>(string destination, T data);
}