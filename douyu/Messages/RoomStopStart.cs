namespace Douyu.Messages
{
    /// <summary>
    /// 房间开播提醒
    /// </summary>
    public class RoomStartStop : AbstractDouyuMessage
    {
        /// <summary>
        /// 房间id 
        /// </summary>
        public string rid { get; set; }

        /// <summary>
        /// 弹幕分组id 
        /// </summary>
        public string gid { get; set; }

        /// <summary>
        /// 直播状态，0-没有直播，1-正在直播 
        /// </summary>
        public string ss { get; set; }

        /// <summary>
        /// 类型 
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// 开关播原因：0-主播开关播，其他值-其他原因 
        /// </summary>
        public string rt { get; set; }

        /// <summary>
        /// 通知类型 
        /// </summary>
        public string notify { get; set; }

        /// <summary>
        /// 关播时间（仅关播时有效） 
        /// </summary>
        public string endtime { get; set; }

        public override string name
        {
            get
            {
                return "房间开播提醒";
            }
        }

        public override string type
        {
            get
            {
                return "rss";
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
