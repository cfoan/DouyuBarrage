using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Douyu.Messages
{
    public class JoinGroup : AbstractDouyuMessage
    {
        public override string name
        {
            get
            {
                return "入组消息";
            }
        }

        public override string type
        {
            get
            {
                return BarrageConstants.TYPE_JOIN_GROUP;
            }
        }

        public override MessageSource source
        {
            get
            {
                return MessageSource.Client;
            }
        }

        public string rid { get; set; }

        public string gid { get; set; }
    }
}
