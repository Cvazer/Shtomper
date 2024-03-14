namespace Shtomper.Client.Impl;

/// <summary>
/// This is a no-op implementation.
/// </summary>
public class NoOpHeartbeatHandler : IHeartbeatHandler
{
    public void Start(Action beat)
    {

    }

    public void Dispose()
    {

    }
}