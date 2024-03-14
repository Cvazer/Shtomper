namespace Shtomper.Client;

public class StompException : Exception
{
    public StompException(string? message): base(message)
    {
    }
}