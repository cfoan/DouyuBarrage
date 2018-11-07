using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;

namespace Douyu.Messages
{
    public class Converters
    {
        private readonly GiftConverter m_giftDecoder = new GiftConverter();
        private readonly BarrageConverter m_barrageDecoder = new BarrageConverter();
        private readonly UserEnterConverter m_userEnterDecoder = new UserEnterConverter();
        private readonly KeepaliveConverter m_keepaliveDecoder = new KeepaliveConverter();
        private readonly SuperBarrageConverter m_superBarrageDecoder = new SuperBarrageConverter();
        private readonly GiftInsideRoomConverter m_giftInsideRoomDecoder = new GiftInsideRoomConverter();
        private readonly RoomStartStopConverter m_roomStartStopDecoder = new RoomStartStopConverter();
        private readonly LoginResponseConverter m_loginResponseDecoder = new LoginResponseConverter();
        private readonly JoinGroupConverter m_joinGroupDecoder = new JoinGroupConverter();
        private readonly LoginRequestConverter m_loginRequestDecoder = new LoginRequestConverter();

        private static Converters instance=new Converters();

        public static Converters Instance { get { return instance; } }

        private Converters()
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
            //todo 其他未知类型
            //System.Console.WriteLine(data);
            return null;
            //throw new InvalidOperationException("Decode:unknown message type");
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public string Encode(AbstractDouyuMessage message)
        {
            if (message.source.Equals(MessageSource.Server))
            {
                throw new NotImplementedException();
            }
            
            switch (message.type)
            {
                case BarrageConstants.TYPE_KEEP_ALIVE:
                    return m_keepaliveDecoder.Encode((Keepalive)message);
                case BarrageConstants.TYPE_JOIN_GROUP:
                    return m_joinGroupDecoder.Encode((JoinGroup)message);
                case BarrageConstants.TYPE_LOGIN_REQUEST:
                    return m_loginRequestDecoder.Encode((LoginRequest)message);

            }
            throw new InvalidOperationException("Encode:unknown message type ");
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

    internal class GiftConverter : DouyuMessageConverter<Gift>
    {

    }

    internal class BarrageConverter : DouyuMessageConverter<Barrage>
    {

    }

    internal class UserEnterConverter : DouyuMessageConverter<UserEnter>
    {

    }

    internal class KeepaliveConverter : DouyuMessageConverter<Keepalive>
    {

    }

    internal class SuperBarrageConverter : DouyuMessageConverter<SuperBarrage>
    {

    }

    internal class GiftInsideRoomConverter : DouyuMessageConverter<GiftInsideRoom>
    {

    }

    internal class RoomStartStopConverter : DouyuMessageConverter<RoomStartStop>
    {

    }

    internal class LoginResponseConverter : DouyuMessageConverter<LoginResponse>
    {

    }

    internal class LoginRequestConverter : DouyuMessageConverter<LoginRequest>
    {

    }

    internal class JoinGroupConverter : DouyuMessageConverter<JoinGroup>
    {

    }
}
