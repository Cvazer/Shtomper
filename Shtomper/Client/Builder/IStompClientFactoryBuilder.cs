namespace Shtomper.Client.Builder;

public interface IStompClientFactoryBuilder<out TFactory>
{
    StompClientBuilder? ClientBuilder { get; set; }
    TFactory Build();
}