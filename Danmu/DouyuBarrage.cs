using DouyuDanmu.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;

namespace DouyuDanmu
{
    internal class FastMethodInvoker
    {
        private object target;
        private HybridDictionary methods = new HybridDictionary();

        public FastMethodInvoker(object target)
        {
            if (target == null) { throw new ArgumentNullException("target"); }
            this.target = target;
        }

        public void AddMethod(string name, MethodInfo info)
        {
            foreach (var key in methods.Keys)
            {
                if (name == (string)key) { return; }
            }
            methods[name] = info;
        }

        public object Invoke(string name, object[] parameters)
        {
            var method = methods[name] as MethodInfo;
            if (method == null) { throw new ArgumentException("method name not found"); }
            return method.Invoke(target, parameters);
        }
    }

    public class DouyuBarrage
    {
        private static string logPath = AppDomain.CurrentDomain.BaseDirectory + "\\log.txt";
        private static object locker = new object();

        private Dictionary<object, FastMethodInvoker> invokerMap = new Dictionary<object, FastMethodInvoker>();

        public static DouyuBarrage instance = new DouyuBarrage();

        public static DouyuBarrage Instance
        {
            get { return instance; }
        }

        private DouyuBarrage()
        {
            LoadAllDouyuMessageHandler();
        }

        private void LoadAllDouyuMessageHandler()
        {
            /**
             * 1.获取所以converter，获取泛型Type
             * 2.获取每个AbstractDouyuMessage的type属性
             * 3.建立type属性和converter两个方法对应关系的对应关系 
             * */
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                if (type.GetInterface("IMessageConverter") != null && !type.IsAbstract)
                {
                    if (type.BaseType == typeof(DouyuMessageConverter<>).MakeGenericType(type.BaseType.GetGenericArguments()))
                    {
                        var genericArgument = type.BaseType.GetGenericArguments()[0];
                        if (genericArgument.BaseType == typeof(AbstractDouyuMessage))
                        {
                            var douyuMessage = (AbstractDouyuMessage)Activator.CreateInstance(genericArgument);
                            FastMethodInvoker methodInvoker = new FastMethodInvoker(type.GetConstructor(Type.EmptyTypes).Invoke(null));
                            methodInvoker.AddMethod("ParseString", type.GetMethod("ParseString"));
                            methodInvoker.AddMethod("DumpsString", type.GetMethod("DumpsString"));
                            invokerMap[douyuMessage.type] = methodInvoker;
                        }
                    }
                }
            }

        }

        public AbstractDouyuMessage Parse(string data)
        {
            if (data.IndexOf("chatmsg") != -1)
            {
                var methodInvoker = invokerMap["chatmsg"];
                var message = (AbstractDouyuMessage)methodInvoker.Invoke("ParseString", new object[] { data });
                return message;
            }
            else if (data.IndexOf("dgb") != -1)
            {
                var methodInvoker = invokerMap["dgb"];
                var message = (AbstractDouyuMessage)methodInvoker.Invoke("ParseString", new object[] { data });
                return message;
            }
            return null;
        }

        public string Dumps(AbstractDouyuMessage douyuMessage)
        {
            throw new NotSupportedException();
        }

        public void ConsoleLog(AbstractDouyuMessage douyuMessage)
        {
            var view = MakeBarrageView(douyuMessage);
            if (!string.IsNullOrWhiteSpace(view))
            {
                Console.WriteLine(view);
            }
        }

        public void ConsoleLog(string rawData)
        {
            var view = MakeBarrageView(Parse(rawData));
            if (!string.IsNullOrWhiteSpace(view))
            {
                Console.WriteLine(view);
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

        public static string MakeBarrageView(AbstractDouyuMessage douyuMessage)
        {
            if (douyuMessage == null) { return ""; }
            switch (douyuMessage.type)
            {
                case "chatmsg":
                    var chatMsg = douyuMessage as Barrage;
                    return string.Format("[弹幕]{0}：{1}", chatMsg.nn, chatMsg.txt);
                case "dgb":
                    var gift = douyuMessage as Gift;
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
