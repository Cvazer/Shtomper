using Shtomper.Frame.Enum;
using Shtomper.Frame.Impl.Client;
using Shtomper.Frame.Impl.Server;

namespace Shtomper.Client.Impl;

public abstract class AbstractStompClientV12 : AbstractStompClientV11
{

    protected AbstractStompClientV12(
        IMessageConverter messageConverter,
        IHeartbeatHandler heartbeatHandler,
        bool receiptMode = false,
        bool nackMode = false
    ) : base(messageConverter, heartbeatHandler, receiptMode, nackMode) { }

    protected override void DoActualAck(Ack obj, Message msg, string? txId, long subId)
    {
        obj.Header(StompHeader.Id, msg.MessageId());
        if (txId != null) obj.Transaction(txId);
        SendMaybeWithReceipt(obj);
    }
}