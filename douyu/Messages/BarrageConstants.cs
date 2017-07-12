namespace Douyu.Messages
{
    public class BarrageConstants
    {
        public const string TYPE_BARRAGE = "chatmsg";

        public const string TYPE_LOGIN_RESPONSE = "loginres";

        public const string TYPE_LOGIN_REQUEST = "loginreq";

        public const string TYPE_GIFT = "dgp";

        public const string TYPE_KEEP_ALIVE = "keepalive";

        public const string TYPE_SUPER_BARRAGE = "ssd";

        public const string TYPE_USER_ENTER = "uenter";

        public const string TYPE_ROOM_START_STOP = "rss";

        public const string TYPE_GIFT_INSIDE_ROOM = "spbc";

        public const string TYPE_JOIN_GROUP = "joingroup";
    }

    public enum MessageSource
    {
        Client,Server
    }
}
