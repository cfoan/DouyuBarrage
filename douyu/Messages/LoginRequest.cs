using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Douyu.Messages
{
    public class LoginRequest : AbstractDouyuMessage
    {
        public override string name
        {
            get
            {
                return "登录请求";
            }
        }

        public override string type
        {
            get
            {
                return BarrageConstants.TYPE_LOGIN_REQUEST;
            }
        }

        public override MessageSource source
        {
            get
            {
                return MessageSource.Client;
            }
        }

        /// <summary>
        /// 所登录的房间id
        /// </summary>
        public string roomid { get; set; }
    }
}
