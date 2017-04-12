using DouyuDanmu.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DouyuDanmu
{ 
    public class DouyuMessage
    {
        private static string logPath = AppDomain.CurrentDomain.BaseDirectory + "\\log.txt";
        private static object locker = new object();

        private Dictionary<object, IMessageConverter> converterMap= new Dictionary<object, IMessageConverter>();

        public void LoadAllDouyuMessage()
        {
            /**
             * 1.获取所以converter，获取泛型Type
             * 2.获取每个Message的type属性
             * 3.建立type属性和converter的对应关系 
             * */
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                if (type.GetInterface("IMessageConverter") != null && !type.IsAbstract)
                {
                    var genericArguments = type.BaseType.GetGenericArguments();
                    foreach (var genericArgument in genericArguments)
                    {
                        if (genericArgument.BaseType == typeof(AbstractDouyuMessage))
                        {
                            var douyuMessageInstance = Activator.CreateInstance(genericArguments[0]);
                            converterMap[genericArguments[0].GetProperty("type").GetValue(douyuMessageInstance)] = (IMessageConverter)type.GetConstructor(Type.EmptyTypes).Invoke(null);
                            continue;
                        }
                    }

                    var ms = type.GetMethod("ParseString");
                    var ms2 = type.GetMethod("DumpsString");
                    Console.WriteLine(ms);
                }
            }

        }

        public void Parse(string data="type@=chatmsg/nn@=aaa/")
        {
            if (data.IndexOf("chatmsg") != -1)
            {
                var converter = converterMap["chatmsg"];
                var message=typeof(IMessageConverter).GetMethod("ParseString").Invoke(converter,new object[] { data });
                Console.WriteLine(message);
            }
        }

        public static void Dumps(string log)
        {
            lock (locker)
            {
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(logPath, true))
                {
                    sw.WriteLine(log);
                }
            }
        }

        internal static string GiftName(string id)
        {
            switch (id)
            {
                case "124":
                    return "电竞三丑";
                case "191":
                    return "100鱼丸";
                case "192":
                    return "赞";
                case "193":
                    return "弱鸡";
                case "194":
                    return "666";
                case "195":
                    return "飞机";
                case "196":
                    return "火箭";
                case "268":
                    return "发财";
                case "380":
                    return "好人卡";
                case "479":
                    return "帐篷";
                case "519":
                    return "呵呵";
                case "520":
                    return "稳";
                case "530":
                    return "天秀";
                case "712":
                    return "棒棒哒";
                case "714":
                    return "怂";
                case "713":
                    return "辣眼睛";
                default:
                    return "unknown";
            }
        }

        public static string ToString(AbstractDouyuMessage message)
        {
            switch (message.type)
            {
                case "chatmsg":
                    var chatMsg = message as Barrage;
                    return string.Format("[弹幕]{0}：{1}", chatMsg.nn, chatMsg.txt);
                case "dgb":
                    var gift = message as Gift;
                    if (GiftName(gift.gfid) == "unknown")
                    {
                        Dumps(gift.Raw);
                    }
                    var giftInfo = string.Format("【{0}】 {1}", GiftName(gift.gfid), !string.IsNullOrWhiteSpace(gift.hits) ? string.Format("{0}连击", gift.hits) : "");
                    return string.Format("[礼物]来自{0} {1}", gift.nn, giftInfo);
                default:
                    return "";
            }
        }

    }
}
