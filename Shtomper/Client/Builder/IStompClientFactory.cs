namespace Shtomper.Client.Builder;

public interface IStompClientFactory<out TClient, TFactoryBuilder>
    where TClient : IStompClient
    where TFactoryBuilder : IStompClientFactoryBuilder<IStompClientFactory<TClient, TFactoryBuilder>>
{
    TClient Create();
}