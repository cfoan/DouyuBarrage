namespace Douyu.Messages
{
    /// <summary>
    /// 超级弹幕消息
    /// </summary>
    public class SuperBarrage : AbstractDouyuMessage
    {
        /// <summary>
        /// 房间 id
        /// </summary>
        public string rid { get; set; }

        /// <summary>
        /// 弹幕分组 id
        /// </summary>
        public string gid { get; set; }

        /// <summary>
        /// 超级弹幕 id
        /// </summary>
        public string sdid { get; set; }

        /// <summary>
        /// 跳转房间 id
        /// </summary>
        public string trid { get; set; }

        /// <summary>
        /// 超级弹幕的内容
        /// </summary>
        public string content { get; set; }

        /// <summary>
        /// 表示为“超级弹幕”消息，固定为 ssd
        /// </summary>
        public override string type
        {
            get
            {
                return "ssd";
            }
        }

        public override string name
        {
            get
            {
                return "超级弹幕";
            }
        }

        public override string ToString()
        {
            return string.Format("[超级弹幕]{0}", content);
        }
    }
}
