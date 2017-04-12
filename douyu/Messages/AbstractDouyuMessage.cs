namespace Douyu
{
    public abstract class AbstractDouyuMessage
    {
        public abstract string type { get; }

        public string Raw { get; set; }
    }
}
