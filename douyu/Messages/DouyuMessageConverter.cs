using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Douyu.Messages
{
    public interface IMessageConverter
    {
        string Name { get; }
    }

    public abstract class DouyuMessageConverter<TDouyuMessage>:IMessageConverter
        where TDouyuMessage : AbstractDouyuMessage,new()
    {
        protected Dictionary<string, PropertyInfo> propertiesMap;

        public string Name
        {
            get
            {
                return typeof(TDouyuMessage).Name;
            }
        }

        public DouyuMessageConverter()
        {
            propertiesMap = new Dictionary<string, PropertyInfo>();
            var properties = typeof(TDouyuMessage).GetProperties();
            Array.ForEach(properties, (property) =>
             {
                 if (property.Name != "type")
                 {
                     propertiesMap[property.Name] = property;
                 }
             });
        }

        protected void SetPropertyValue(TDouyuMessage message,string propertyName, object value)
        {
            if (propertiesMap.ContainsKey(propertyName))
            {
                propertiesMap[propertyName].SetValue(message, value);
            }
        }

        protected string Unescape(string pre)
        {
            if (pre == null) { throw new ArgumentNullException(); }
            return pre.Replace("@S", "/").Replace("@A", "@");
        }

        protected string Escape(string pre)
        {
            if (pre == null) { throw new ArgumentNullException(); }
            return pre.Replace("/", "@S").Replace("@", "@A");
        }

        public TDouyuMessage ParseString(string douyuMessage)
        {
            TDouyuMessage message = new TDouyuMessage();
            message.Raw = douyuMessage;
            var differentParts = douyuMessage.Split(new string[] { "@=", "/" }, StringSplitOptions.None);
            for (int i = 0; i < differentParts.Length / 2; i++)
            {
                var key = Unescape(differentParts[2 * i]);
                var value = Unescape(differentParts[2 * i + 1]);
                SetPropertyValue(message, key, value);
            }
            return message;
        }

        public abstract string DumpsString(TDouyuMessage douyuMessage);
    }
}
