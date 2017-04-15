using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;

namespace Douyu.Messages
{
    public class Decoders
    {
        private Dictionary<object, FastMethodInvoker> invokerMap = new Dictionary<object, FastMethodInvoker>();

        private static Decoders instance=new Decoders();

        public static Decoders Instance { get { return instance; } }

        private Decoders()
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

        public AbstractDouyuMessage Parse(string data)
        {
            AbstractDouyuMessage douyuMessage = null;
            var type = GetMessageType(data);
            FastMethodInvoker methodInvoker = null;
            if (invokerMap.TryGetValue(type, out methodInvoker))
            {
                douyuMessage = (AbstractDouyuMessage)methodInvoker.Invoke("ParseString", new object[] { data });
            }
            return douyuMessage;
        }

        public AbstractDouyuMessage[] Parse(string[] datas)
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
            return douyuMessagesLs.ToArray();
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
    }

    public class GiftDecoder : DouyuMessageDecoder<Gift> { }

    public class BarrageDecoder : DouyuMessageDecoder<Barrage> { }

    public class UserEnterDecoder : DouyuMessageDecoder<UserEnter> { }

    public class KeepaliveDecoder : DouyuMessageDecoder<Keepalive> { }

    public class SuperBarrageDecoder : DouyuMessageDecoder<SuperBarrage> { }

    public class GiftInsideRoomDecoder : DouyuMessageDecoder<GiftInsideRoom> { }

    public class RoomStartStopDecoder : DouyuMessageDecoder<RoomStartStop> { }

    public class LoginResponseDecoder : DouyuMessageDecoder<LoginResponse> { }

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
}
