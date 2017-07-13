using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Douyu.Messages
{
    /// <summary>
    /// 消息转换类
    /// </summary>
    public interface IMessageConverter
    {
        string Name { get; }
    }

    public interface IMessageConverter<TRaw,TDouyuMessage> : IMessageConverter
    {
        TDouyuMessage Decode(TRaw data);

        TRaw Encode(TDouyuMessage message);
    }

    /// <summary>
    /// 斗鱼消息解码抽象类
    /// </summary>
    /// <typeparam name="TDouyuMessage"></typeparam>
    public abstract class DouyuMessageConverter<TDouyuMessage> : IMessageConverter<string,TDouyuMessage>
        where TDouyuMessage : AbstractDouyuMessage, new()
    {
        internal const string KeyValueSplitor = "@=";
        internal const string KeyValueArraySplitor = "/";

        protected ConcurrentDictionary<string, PropertyInfo> propertiesMap;

        public DouyuMessageConverter()
        {
            propertiesMap = new ConcurrentDictionary<string, PropertyInfo>();
            var properties = typeof(TDouyuMessage).GetProperties();
            Array.ForEach(properties, (property) =>
             {
                 if (property.Name != "type" && property.Name != "source" && property.Name != "name" && property.Name != "raw")
                 {
                     propertiesMap[property.Name] = property;
                 }
             });
        }

        public string Name
        {
            get
            {
                return typeof(TDouyuMessage).Name;
            }
        }

        protected void SetPropertyValue(TDouyuMessage message, string propertyName, object value)
        {
            if ("name".Equals(propertyName) || "type".Equals(propertyName))
                return;

            if (propertiesMap.ContainsKey(propertyName))
            {
                propertiesMap[propertyName].SetValue(message, value,null);
            }
        }

        protected object GetPropertyValue(TDouyuMessage message, string propertyName)
        {
            return propertiesMap.ContainsKey(propertyName) ? propertiesMap[propertyName].GetValue(message, null) : null;
        }

        /// <summary>
        /// 反转义
        /// </summary>
        /// <param name="pre"></param>
        /// <returns>反转义后的数据</returns>
        public static string Unescape(string pre)
        {
            if (pre == null) { throw new ArgumentNullException(); }
            return pre.Replace("@S", "/").Replace("@A", "@");
        }

        public static string Escape(string pre)
        {
            if (pre == null) { throw new ArgumentNullException(); }
            return pre.Replace("/", "@S").Replace("@", "@A");
        }

        /// <summary>
        /// 把字符串类型的数据转换成对应的消息实体
        /// </summary>
        /// <param name="douyuMessage"></param>
        /// <returns>斗鱼消息</returns>
        public TDouyuMessage Decode(string douyuMessage)
        {
            TDouyuMessage m = new TDouyuMessage();
            m.raw = douyuMessage;
            var differentParts = douyuMessage.Split(new string[] { "@=", "/" }, StringSplitOptions.None);
            for (int i = 0; i < differentParts.Length / 2; i++)
            {
                var key = Unescape(differentParts[2 * i]);
                var value = Unescape(differentParts[2 * i + 1]);
                SetPropertyValue(m, key, value);
            }
            return m;
        }

        public string Encode(TDouyuMessage message)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($"type{KeyValueSplitor}{message.type}{KeyValueArraySplitor}");
            foreach (var key in propertiesMap.Keys)
            {
                var value=GetPropertyValue(message, key);
                if (value == null) { continue; }
                if (string.IsNullOrWhiteSpace(((string)value))) { continue; }
                var escapedKey = Escape(key);
                var escapedValue = Escape((string)value);
                builder.Append($"{escapedKey}{KeyValueSplitor}{escapedValue}{KeyValueArraySplitor}");
            }
            return builder.ToString();
        }
    }
}
