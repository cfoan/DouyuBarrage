namespace Douyu.Messages
{
    public class LoginResponse : AbstractDouyuMessage
    {
        /// <summary>
        /// 用户ID 
        /// </summary>
        public string userid { get; set; }

        /// <summary>
        /// 房间权限组 
        /// </summary>
        public string roomgroup { get; set; }

        /// <summary>
        /// 平台权限组 
        /// </summary>
        public string pg { get; set; }

        /// <summary>
        /// 会话ID 
        /// </summary>
        public string sessionId { get; set; }

        /// <summary>
        /// 用户名 
        /// </summary>
        public string username { get; set; }

        /// <summary>
        /// 用户昵称 
        /// </summary>
        public string nickname { get; set; }

        /// <summary>
        /// 是否已在房间签到 
        /// </summary>
        public string is_signed { get; set; }

        /// <summary>
        /// 日总签到次数 
        /// </summary>
        public string signed_count { get; set; }

        /// <summary>
        /// 直播状态 
        /// </summary>
        public string live_stat { get; set; }

        /// <summary>
        /// 是否需要手机验证 
        /// </summary>
        public string npv { get; set; }

        /// <summary>
        /// 最高酬勤等级 
        /// </summary>
        public string best_dlev { get; set; }

        /// <summary>
        /// 酬勤等级 
        /// </summary>
        public string cur_lev { get; set; }

        public override string name
        {
            get
            {
                return "登录响应";
            }
        }

        /// <summary>
        /// 表示为“登出”消息，固定为loginres 
        /// </summary>
        public override string type
        {
            get
            {
                return "loginres";
            }
        }

        public override MessageSource source
        {
            get
            {
                return MessageSource.Server;
            }
        }
    }
}
