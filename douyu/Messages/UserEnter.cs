namespace Douyu.Messages
{
    /// <summary>
    /// 用户进房通知消息
    /// </summary>
    public class UserEnter : AbstractDouyuMessage
    {
        /// <summary>
        /// 表示为“用户进房通知”消息，固定为 uenter
        /// </summary>
        public override string type
        {
            get
            {
                return "uenter";
            }
        }

        public override string name
        {
            get
            {
                return "用户进房通知";
            }
        }

        public override MessageSource source
        {
            get
            {
                return MessageSource.Server;
            }
        }

        // <summary>
        /// 房间id
        /// </summary>
        public string rid { get; set; }

        /// <summary>
        /// 弹幕分组 ID
        /// </summary>
        public string gid { get; set; }

        /// <summary>
        /// 发送者id
        /// </summary>
        public string uid { get; set; }

        /// <summary>
        /// 发送者昵称
        /// </summary>
        public string nn { get; set; }

        /// <summary>
        /// 战斗力
        /// </summary>
        public string str { get; set; }

        /// <summary>
        /// 用户等级
        /// </summary>
        public string level { get; set; }

        /// <summary>
        /// 礼物头衔：默认值0（表示没有头衔）
        /// </summary>
        public string gt { get; set; }

        /// <summary>
        /// 房间权限组：默认值1（表示普通权限用户）
        /// </summary>
        public string rg { get; set; }

        /// <summary>
        /// 平台权限组：默认值1（表示普通权限用户）
        /// </summary>
        public string pg { get; set; }

        /// <summary>
        /// 酬勤等级：默认值0（表示没有酬勤）
        /// </summary>
        public string dlv { get; set; }

        /// <summary>
        /// 酬勤数量：默认值0（表示没有酬勤数量）
        /// </summary>
        public string dc { get; set; }

        /// <summary>
        /// 最高酬勤等级：默认值0（表示全站都没有酬勤）
        /// </summary>
        public string bdlv { get; set; }
    }
}
