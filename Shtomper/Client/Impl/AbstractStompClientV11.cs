using Shtomper.Client.Enum;
using Shtomper.Frame;
using Shtomper.Frame.Enum;
using Shtomper.Frame.Impl.Client;
using Shtomper.Frame.Impl.Server;

namespace Shtomper.Client.Impl;

public abstract class AbstractStompClientV11 : AbstractStompClientV10
{

    protected AbstractStompClientV11(
        IMessageConverter messageConverter,
        IHeartbeatHandler heartbeatHandler,
        bool receiptMode = false
    ) : base(messageConverter, heartbeatHandler, receiptMode)
    {
    }

    protected override void AckMessage(AckMode ackMode, Message msg, string? txId, string destination)
    {
        if (ackMode != AckMode.ClientIndividual) return;
        DoActualAck(msg, txId);
    }

    protected override void DoActualAck(Message msg, string? txId)
    {
        var ack = new Ack(msg.MessageId());
        if (txId != null) ack.Transaction(txId);
        ack.Subscription(msg.Subscription()!.Value);
        SendMaybeWithReceipt(ack);
    }

    protected override void DoPostMsgHandle(Message? lastMsg)
    {
        base.DoPostMsgHandle(lastMsg);
        if (lastMsg == null) return;
        if (SubInfo[lastMsg!.Destination()][lastMsg.Subscription()!.Value] != AckMode.Client) return;
        DoActualAck(lastMsg, lastMsg.Transaction());
    }

    // protected override bool AssessForDisconnect(StompFrame frame) => false;
    //
    // protected override bool AssessReceiptMode(StompFrame frame) =>
    //     frame.Data.Command.Equals(Command.Disconnect) || ReceiptMode;
}