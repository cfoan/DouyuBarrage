namespace Douyu
{
    /// <summary>
    /// 斗鱼弹幕服务器消息抽象类
    /// </summary>
    public abstract class AbstractDouyuMessage
    {
        /// <summary>
        /// 消息类型
        /// </summary>
        public abstract string type { get; }

        /// <summary>
        /// 消息名称
        /// </summary>
        public abstract string name { get; }

        /// <summary>
        /// 原始消息
        /// </summary>
        public string raw { get; set; }
    }
}
