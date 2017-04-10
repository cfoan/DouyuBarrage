using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DouyuDanmu
{
    public class Packet
    {
        public int LengthA { get; set; }

        public int LengthB { get; set; }

        public int Type { get; set; }

        public string Data { get; set; }
    }
}
