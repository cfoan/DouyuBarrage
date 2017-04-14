namespace Douyu.Messages
{
    /// <summary>
    /// 房间内礼物广播
    /// </summary>
    public class GiftInsideRoom : AbstractDouyuMessage
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
        /// 赠送者昵称 
        /// </summary>
        public string sn { get; set; }

        /// <summary>
        /// 受赠者昵称 
        /// </summary>
        public string dn { get; set; }

        /// <summary>
        /// 礼物名称 
        /// </summary>
        public string gn { get; set; }

        /// <summary>
        /// 礼物数量 
        /// </summary>
        public string gc { get; set; }

        /// <summary>
        /// 赠送房间 
        /// </summary>
        public string drid { get; set; }

        /// <summary>
        /// 广播样式 
        /// </summary>
        public string gs { get; set; }

        /// <summary>
        /// 是否有礼包（0-无礼包，1-有礼包） 
        /// </summary>
        public string gb { get; set; }

        /// <summary>
        /// 广播展现样式（1-火箭，2-飞机） 
        /// </summary>
        public string es { get; set; }

        /// <summary>
        /// 礼物id 
        /// </summary>
        public string gfid { get; set; }

        /// <summary>
        /// 特效id 
        /// </summary>
        public string eid { get; set; }

        public override string name
        {
            get
            {
                return "房间内礼物";
            }
        }

        public override string type
        {
            get
            {
                return "spbc";
            }
        }
    }
}
