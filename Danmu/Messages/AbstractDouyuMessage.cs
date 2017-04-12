namespace DouyuDanmu
{
    public abstract class AbstractDouyuMessage
    {
        public AbstractDouyuMessage()
        {

        }
        public abstract string type { get; }

        public string Raw { get; set; }
    }
}
