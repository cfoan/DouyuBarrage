using Douyu.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;

namespace Douyu
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

    /// <summary>
    /// 斗鱼消息处理
    /// </summary>
    public class DouyuBarrage
    {
        private static string logPath = AppDomain.CurrentDomain.BaseDirectory + "\\log.txt";
        private static object locker = new object();

        private Dictionary<object, FastMethodInvoker> invokerMap = new Dictionary<object, FastMethodInvoker>();

        private static DouyuBarrage instance = new DouyuBarrage();

        public static DouyuBarrage Instance
        {
            get { return instance; }
        }

        private DouyuBarrage()
        {
            LoadAllDouyuMessageDecoder();
        }

        private void LoadAllDouyuMessageDecoder()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type[] types = assembly.GetTypes();
            foreach (Type type in types)
            {
                if (type.GetInterface("IMessageConverter") != null && !type.IsAbstract)
                {
                    if (type.BaseType == typeof(DouyuMessageDecoder<>).MakeGenericType(type.BaseType.GetGenericArguments()))
                    {
                        var genericArgument = type.BaseType.GetGenericArguments()[0];
                        if (genericArgument.BaseType == typeof(AbstractDouyuMessage))
                        {
                            var douyuMessage = (AbstractDouyuMessage)Activator.CreateInstance(genericArgument);
                            FastMethodInvoker methodInvoker = new FastMethodInvoker(type.GetConstructor(Type.EmptyTypes).Invoke(null));
                            methodInvoker.AddMethod("ParseString", type.GetMethod("ParseString"));
                            invokerMap[douyuMessage.type] = methodInvoker;
                        }
                    }
                }
            }

        }

        private string GetMessageType(string data)
        {
            if (data == null) { throw new ArgumentNullException("data"); }
            if (data.IndexOf("/") != -1)
            {
                var firstKeyValue = data.Substring(0, data.IndexOf("/")).Split(new string[] { "@=" }, StringSplitOptions.RemoveEmptyEntries);
                if (firstKeyValue.Length == 2)
                {
                    return firstKeyValue[1];
                }
            }
            return "unknown";
        }

        public DouyuBarrage Parse(string data,out AbstractDouyuMessage douyuMessage)
        {
            douyuMessage = null;
            var type = GetMessageType(data);
            FastMethodInvoker methodInvoker = null;
            if (invokerMap.TryGetValue(type, out methodInvoker))
            {
                douyuMessage=(AbstractDouyuMessage)methodInvoker.Invoke("ParseString", new object[] { data });
            }
            return this;
        }

        public DouyuBarrage Parse(string[] datas, out AbstractDouyuMessage[] douyuMessages)
        {
            List<AbstractDouyuMessage> douyuMessagesLs = new List<AbstractDouyuMessage>(datas.Length);
            Array.ForEach(datas, (data) =>
             {
                 var type = GetMessageType(data);
                 FastMethodInvoker methodInvoker = null;
                 if (invokerMap.TryGetValue(type, out methodInvoker))
                 {
                     var douyuMessage = (AbstractDouyuMessage)methodInvoker.Invoke("ParseString", new object[] { data });
                     douyuMessagesLs.Add(douyuMessage);
                 }
             });
            douyuMessages = douyuMessagesLs.ToArray();
            return this;
        }

        /// <summary>
        /// 控制台打印，如果传入的参数为null,则不打印
        /// </summary>
        /// <param name="douyuMessage"></param>
        public void ShowBarrageView(AbstractDouyuMessage douyuMessage)
        {
            if (douyuMessage == null) { return; }
            ShowBarrageViewInternal(douyuMessage);
        }

        public void ShowBarrageView(AbstractDouyuMessage[] douyuMessages)
        {
            if (douyuMessages == null) { return; }
            ShowBarrageViewInternal(douyuMessages);
        }

        private void ShowBarrageViewInternal(params AbstractDouyuMessage[]douyuMessages)
        {
            if (douyuMessages == null) { return; }
            StringBuilder sb = new StringBuilder();
            Array.ForEach(douyuMessages, (msg) =>
             {
                 var msgView = MessageView(msg);
                 if (!string.IsNullOrWhiteSpace(msgView))
                 {
                     sb.AppendLine(msgView);
                 }
             });
            Console.Write(sb.ToString());
        }

        internal string MessageView(AbstractDouyuMessage douyuMessage)
        {
            switch (douyuMessage.type)
            {
                case "chatmsg":
                    var chatMsg = douyuMessage as Barrage;
                    return string.Format("[弹幕]{0}：{1}", chatMsg.nn, chatMsg.txt);
                case "dgb":
                    var gift = douyuMessage as Gift;
                    if (GiftUtil.GiftName(gift.gfid) == "unknown")
                    {
                        Dumps(gift.raw);
                    }
                    var giftInfo = string.Format("【{0}】 {1}", GiftUtil.GiftName(gift.gfid), !string.IsNullOrWhiteSpace(gift.hits) ?
                        string.Format("{0}连击", gift.hits) : "");
                    return string.Format("[礼物]来自{0} {1}", gift.nn, giftInfo);
                case "ssd":
                    var superBarrage = douyuMessage as SuperBarrage;
                    return string.Format("[超级弹幕]{0}", superBarrage.content);
                default:
                    break;
            }
            return "";
        }

        public static void Dumps(AbstractDouyuMessage douyuMessage)
        {
            if (douyuMessage == null) { return; }
            Dumps(douyuMessage.ToString());
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

    }
}
