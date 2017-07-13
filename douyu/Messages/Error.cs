using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Douyu.Messages
{
    public class Error : AbstractDouyuMessage
    {
        public override string name
        {
            get
            {
                return "";
            }
        }

        public override MessageSource source
        {
            get
            {
                return MessageSource.Server;
            }
        }

        public override string type
        {
            get
            {
                return "error";
            }
        }

        public string code { get; set; }
    }
}
