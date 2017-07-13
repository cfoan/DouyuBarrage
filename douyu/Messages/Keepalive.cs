namespace Douyu.Messages
{
    public class Keepalive : AbstractDouyuMessage
    {
        /// <summary>
        /// 当前 unix 时间戳(秒)
        /// </summary>
        public string tick { get; set; }

        public override string type
        {
            get
            {
                return "keepalive";
            }
        }

        public override string name
        {
            get
            {
                return "心跳";
            }
        }

        public override MessageSource source
        {
            get
            {
                return MessageSource.Client;
            }
        }
    }
}
