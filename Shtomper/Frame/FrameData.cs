using System.Text.RegularExpressions;
using Shtomper.Frame.Enum;

namespace Shtomper.Frame;

public readonly struct FrameData
{
    private static readonly Regex FrameDataRegex = new(
        @"^([^\n]+)\n(?:([^:]+[:][^\n]+)\n)*\n*(?:(.+))*\x00{1}$"
    );
    
    public Command Command { get; }
    public List<(string, string)> Headers { get; }
    public string? Body { get; }

    public FrameData(Command command, Dictionary<string, string>? headers = null, string? body = null)
        : this(command, headers?.Select(pair => (pair.Key, pair.Value)).ToList(), body)
    {
    }

    private FrameData(Command command, List<(string, string)>? headers = null, string? body = null) =>
        (Command, Headers, Body) = (command, headers ?? new List<(string, string)>(), body);

    public static FrameData FromString(string data)
    {
        if (!FrameDataRegex.IsMatch(data))
        {
            throw new ArgumentException("Malformed data");
        }
        
        var match = FrameDataRegex.Match(data);
        if (match.Groups.Count < 3)
        {
            throw new ArgumentException("Invalid data");
        }
        
        if (!System.Enum.TryParse(match.Groups[1].Value, true, out Command cmd))
        {
            throw new ArgumentException("Invalid stomp command");
        }

        var headers = match.Groups[2].Captures
            .Select(it => it.Value.Split(":"))
            .Select(it => (Decode(it[0]), Decode(it[1])))
            .ToList();
        
        var body = match.Groups.Count >= 4 
            ? match.Groups[3].Value 
            : null;
        
        return new FrameData(cmd, headers, body);
    }

    public string Stringify()
    {
        var cmd = Command.ToString().ToUpper();
        
        var headers = Headers
            .Select(pair => Encode(pair.Item1) + ":" + Encode(pair.Item2))
            .Aggregate("", (s1, s2) => s1 + "\n" + s2);

        if (Headers.Count != 0)
        {
            headers = headers.Remove(0, 1);
        }

        if (Headers.Count != 0)
        {
            headers += "\n";
        }

        if (Body == null)
        {
            return $"{cmd}\n{headers}\n" + char.MinValue;
        }
        
        return $"{cmd}\n{headers}\n{Body}" + char.MinValue;
    }

    public static string Encode(string data) => data
        .Replace("\r", "\\r")
        .Replace("\n", "\\n")
        .Replace(":", "\\c")
        .Replace("\\", "\\\\");

    private static string Decode(string data) => data
        .Replace("\\r", "\r")
        .Replace("\\n", "\n")
        .Replace("\\c", ":")
        .Replace("\\\\", "\\");
}