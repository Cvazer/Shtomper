using Shtomper.Client.Enum;
using Shtomper.Frame.Impl.Client;
using Shtomper.Frame.Impl.Server;

namespace Shtomper.Client.Impl;

public abstract class AbstractStompClientV11 : AbstractStompClientV10
{

    protected AbstractStompClientV11(
        IMessageConverter messageConverter,
        IHeartbeatHandler heartbeatHandler,
        bool receiptMode = false,
        bool nackMode = false
    ) : base(messageConverter, heartbeatHandler, receiptMode, nackMode) { }

    protected override void AckMessage(AckMode ackMode, Message msg, string? txId, long subId)
    {
        if (ackMode != AckMode.ClientIndividual) return;
        DoActualAck(msg, txId, subId);
    }

    protected override void DoActualAck(Ack obj, Message msg, string? txId, long subId)
    {
        obj.MessageId(msg.MessageId());
        obj.Subscription(msg.Subscription()!.Value);
        if (txId != null) obj.Transaction(txId);
        SendMaybeWithReceipt(obj);
    }

    protected override void DoPostMsgHandle(Message? lastMsg)
    {
        base.DoPostMsgHandle(lastMsg);

        if (lastMsg == null) return;
        if (SubInfo[lastMsg.Destination()][lastMsg.Subscription()!.Value] != AckMode.Client) return;
        DoActualAck(lastMsg, lastMsg.Transaction(), lastMsg.Subscription()!.Value);
    }
}