using Shtomper.Frame;

namespace Shtomper.Client;

public interface IHeartbeatHandler: IDisposable
{
    void Start(Action beat);
}