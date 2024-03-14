using Shtomper.Frame.Impl.Client;
using Shtomper.Frame.Impl.Server;

namespace Shtomper.Client;

public interface IMessageConverter
{
    Send Convert<T>(string destination, T data, Dictionary<string, string>? userDefinedHeaders = null);
    T Convert<T>(Message message);
}