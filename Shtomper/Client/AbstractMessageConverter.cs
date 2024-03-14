using Shtomper.Frame;
using Shtomper.Frame.Impl.Client;
using Shtomper.Frame.Impl.Server;

namespace Shtomper.Client;

public abstract class AbstractMessageConverter : IMessageConverter
{

    public Send Convert<T>(string destination, T data, Dictionary<string, string>? userDefinedHeaders = null)
    {
        return new Send(destination, Serialize(data), GetContentType(), userDefinedHeaders: userDefinedHeaders);
    }

    public T Convert<T>(Message message)
    {
        return Deserialize<T>(message.Body()!);
    }

    protected abstract T Deserialize<T>(string data);
    protected abstract string Serialize<T>(T data);
    protected abstract ContentType GetContentType();
}