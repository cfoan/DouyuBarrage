using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
