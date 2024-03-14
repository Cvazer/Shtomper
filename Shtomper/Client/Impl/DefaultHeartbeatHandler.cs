namespace Shtomper.Client.Impl;

public class DefaultHeartbeatHandler : IHeartbeatHandler
{
    private readonly CancellationTokenSource _cancellation = new();
    private readonly int _hb;

    public DefaultHeartbeatHandler(int hbInMillis) => _hb = hbInMillis;

    public async void Start(Action beat)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(_hb));
        
        try
        {
            while (await timer.WaitForNextTickAsync(_cancellation.Token) &&
                   !_cancellation.Token.IsCancellationRequested)
            {
                beat.Invoke();
            }
        }

        catch (OperationCanceledException) { }
    }

    public void Dispose()
    {
        _cancellation.Cancel();
        _cancellation.Dispose();
    }
}