using Newtonsoft.Json;
using Shtomper.Client;
using Shtomper.Frame;

namespace Shtomper_Converter_NewtonsoftJson;

public class NewtonsoftJsonMessageConverter: AbstractMessageConverter
{

    protected override T Deserialize<T>(string data) where T : default
    {
        if (typeof(T) == typeof(string))
        {
            return (T)System.Convert.ChangeType(data, typeof(T));
        }
        return JsonConvert.DeserializeObject<T>(data)!;
    }

    protected override string Serialize<T>(T data)
    {
        return JsonConvert.SerializeObject(data);
    }

    protected override ContentType GetContentType()
    {
        return ContentType.ApplicationJson;
    }
}