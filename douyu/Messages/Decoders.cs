namespace Douyu.Messages
{
    public class Decoders
    {
        public class GiftDecoder : DouyuMessageDecoder<Gift> { }

        public class BarrageDecoder : DouyuMessageDecoder<Barrage> { }

        public class UserEnterDecoder : DouyuMessageDecoder<UserEnter> { }

        public class KeepaliveDecoder : DouyuMessageDecoder<Keepalive> { }

        public class SuperBarrageDecoder:DouyuMessageDecoder<SuperBarrage> { }

        public class GiftInsideRoomDecoder : DouyuMessageDecoder<GiftInsideRoom> { }

        public class RoomStartStopDecoder : DouyuMessageDecoder<RoomStartStop> { }
    }
}
