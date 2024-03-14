using System.Text;
using Shtomper.Frame;
using Shtomper.Frame.Impl.Client;
using Xunit;

namespace Test.Frame;

public class ConnectTest
{

    [Fact(DisplayName = "Sanity Check")]
    public void SanityCheck()
    {
        var connect = new Connect(
            hostname: "/", 
            login: "guest", 
            passcode: "guest",
            heartBeatCapable: 100,
            heartBeatDesired: 100
        );

        var expected = new StringBuilder()
            .Append("CONNECT").AppendLine()
            .Append("accept-version:1.0,1.1,1.2").AppendLine()
            .Append("host:/").AppendLine()
            .Append("login:guest").AppendLine()
            .Append("passcode:guest").AppendLine()
            .Append("heart-beat:100,100").AppendLine()
            .AppendLine()
            .Append(char.MinValue);
        
        Assert.Equal(expected.ToString(), connect.ToString());
    }

    [Fact(DisplayName = "When creating with defaults => CorrectValue")]
    public void Creating_WithDefaults_CorrectValue()
    {
        var connect = new Connect(hostname: "/");
        var expectedHeartBeat = $"{Connect.DefaultCapableHeartBeat},{Connect.DefaultDesiredHeartBeat}";

        Assert.Null(connect.Header(StompHeader.Login));
        Assert.Null(connect.Header(StompHeader.Passcode));
        Assert.Equal(expectedHeartBeat, connect.Header(StompHeader.HeartBeat));
    }
    
}