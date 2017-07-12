using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;

namespace Douyu.Messages
{
    public class Decoders
    {
        //Func<string, Gift> GiftParser= new GiftDecoder().Decode;
        private readonly GiftDecoder m_giftDecoder = new GiftDecoder();
        private readonly BarrageDecoder m_barrageDecoder = new BarrageDecoder();
        private readonly UserEnterDecoder m_userEnterDecoder = new UserEnterDecoder();
        private readonly KeepaliveDecoder m_keepaliveDecoder = new KeepaliveDecoder();
        private readonly SuperBarrageDecoder m_superBarrageDecoder = new SuperBarrageDecoder();
        private readonly GiftInsideRoomDecoder m_giftInsideRoomDecoder = new GiftInsideRoomDecoder();
        private readonly RoomStartStopDecoder m_roomStartStopDecoder = new RoomStartStopDecoder();
        private readonly LoginResponseDecoder m_loginResponseDecoder = new LoginResponseDecoder();
        private static Decoders instance=new Decoders();

        public static Decoders Instance { get { return instance; } }

        private Decoders()
        {

        }

        public AbstractDouyuMessage Parse(string data)
        {
            var type = GetMessageType(data);
            switch (type)
            {
                case BarrageConstants.TYPE_GIFT:
                    return m_giftDecoder.Decode(data);
                case BarrageConstants.TYPE_BARRAGE:
                    return m_barrageDecoder.Decode(data);
                case BarrageConstants.TYPE_GIFT_INSIDE_ROOM:
                    return m_giftInsideRoomDecoder.Decode(data);
                case BarrageConstants.TYPE_KEEP_ALIVE:
                    return m_keepaliveDecoder.Decode(data);
                case BarrageConstants.TYPE_LOGIN_RESPONSE:
                    return m_loginResponseDecoder.Decode(data);
                case BarrageConstants.TYPE_ROOM_START_STOP:
                    return m_roomStartStopDecoder.Decode(data);
                case BarrageConstants.TYPE_SUPER_BARRAGE:
                    return m_superBarrageDecoder.Decode(data);
                case BarrageConstants.TYPE_USER_ENTER:
                    return m_userEnterDecoder.Decode(data);

            }
            throw new InvalidOperationException("unknown message type");
        }

        public AbstractDouyuMessage[] Parse(string[] datas)
        {
            List<AbstractDouyuMessage> douyuMessagesLs = new List<AbstractDouyuMessage>(datas.Length);
            Array.ForEach(datas, (data) =>
            {
                douyuMessagesLs.Add(Parse(data));
            });
            return douyuMessagesLs.ToArray();
        }

        private string GetMessageType(string data)
        {
            if (data == null) { throw new ArgumentNullException("data"); }
            if (data.IndexOf("/") != -1)
            {
                var firstKeyValue = data.Substring(0, data.IndexOf("/")).Split(new string[] { "@=" }, StringSplitOptions.RemoveEmptyEntries);
                if (firstKeyValue.Length == 2)
                {
                    return firstKeyValue[1];
                }
            }
            return "unknown";
        }
    }

    internal class GiftDecoder : DouyuMessageDecoder<Gift> { }

    internal class BarrageDecoder : DouyuMessageDecoder<Barrage> { }

    internal class UserEnterDecoder : DouyuMessageDecoder<UserEnter> { }

    internal class KeepaliveDecoder : DouyuMessageDecoder<Keepalive> { }

    internal class SuperBarrageDecoder : DouyuMessageDecoder<SuperBarrage> { }

    internal class GiftInsideRoomDecoder : DouyuMessageDecoder<GiftInsideRoom> { }

    internal class RoomStartStopDecoder : DouyuMessageDecoder<RoomStartStop> { }

    internal class LoginResponseDecoder : DouyuMessageDecoder<LoginResponse> { }
}
