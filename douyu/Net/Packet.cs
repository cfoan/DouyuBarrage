namespace Douyu.Net
{
    /// <summary>
    /// 包结构
    /// </summary>
    public class Packet
    {
        /// <summary>
        /// 同LengthB
        /// </summary>
        public int LengthA { get; set; }

        /// <summary>
        /// 包长度（不含本身）
        /// </summary>
        public int LengthB { get; set; }

        /// <summary>
        /// 包类型标识
        /// </summary>
        public int Flag { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public string Data { get; set; }
    }
}
