using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Douyu.Messages
{
    public class Keepalive : AbstractDouyuMessage
    {
        /// <summary>
        /// 当前 unix 时间戳(秒)
        /// </summary>
        public string tick { get; set; }

        public override string type
        {
            get
            {
                return "keepalive";
            }
        }

        public override string name
        {
            get
            {
                return "心跳";
            }
        }
    }
}
