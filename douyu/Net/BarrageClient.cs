using Douyu.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Douyu.Net
{
    public class BarrageClient
    {
        public Action ConnectHandler;
        public Action DisconnectHandler;
        public Action OnDataArriveHandler;

        public List<Func<AbstractDouyuMessage, bool>> m_filters;
        public ConcurrentDictionary<string, List<Action<AbstractDouyuMessage>>> m_TypeToHandler;
        private const int LoginTimeout = 5000;
        private const int KeepAliveTimeout = 45 * 1000;
        private BarrageConnection m_client;
        private Timer m_timer;
        private object m_locker;
        private TaskCompletionSource<object> loginTcs;
        private Action<LoginResponse> loginResponseHandler;
        public BarrageClient()
        {
            m_locker = new object();
            m_timer = new Timer(Heatbeat, null, Timeout.Infinite, Timeout.Infinite);
            m_filters = new List<Func<AbstractDouyuMessage, bool>>();
            m_TypeToHandler = new ConcurrentDictionary<string, List<Action<AbstractDouyuMessage>>>();
            m_client = new BarrageConnection();
            m_client.OnDataArrive += m_client_OnDataArrive;
            m_client.OnConnectToServer += m_client_OnConnectToServer;
            m_client.OnDisconnectFromServer += m_client_OnDisconnectFromServer;

            loginResponseHandler=(message)=> loginTcs?.SetResult(true);
            AddHandler(BarrageConstants.TYPE_LOGIN_RESPONSE, (message) =>loginResponseHandler?.Invoke((LoginResponse)message));
        }

        public BarrageClient Start()
        {
            if (m_client.Connected) { return this; }
            m_client.Connect();
            m_client.Send(Converters.Instance.Encode(new LoginRequest()));
            loginTcs = new TaskCompletionSource<object>();
            WaitImpl(loginTcs.Task, LoginTimeout);
            return this;
        }

        public BarrageClient EnterRoom(string roomId, string groupId = "-9999")
        {
#if DEBUG
            Console.WriteLine("try enter room {0}...", roomId);
#endif
            JoinGroup joinGroup = new JoinGroup();
            joinGroup.gid = groupId;
            joinGroup.rid = roomId;
            m_client.Send(Converters.Instance.Encode(joinGroup));
            return this;
        }

        private void Heatbeat(object sender)
        {
            var keepAlive = new Keepalive() { tick = (Utils.CurrentTimestampUtc() / 1000).ToString() };//取的秒数
            m_client.Send(Converters.Instance.Encode(keepAlive));
        }

        public void Stop()
        {
            m_client.Close();
        }

        private void m_client_OnDisconnectFromServer()
        {
            m_timer.Change(Timeout.Infinite, Timeout.Infinite);
            DisconnectHandler?.Invoke();
        }

        private void m_client_OnConnectToServer()
        {
            m_timer.Change(0, KeepAliveTimeout);
            ConnectHandler?.Invoke();
        }

        private void m_client_OnDataArrive(Packet[] packets)
        {
            if (packets == null || packets.Length == 0) { return; }
            var datas = packets.Select((packet) => { return packet.Data; });
            var douyuMessages = Converters.Instance.Parse(datas.ToArray());
            Array.ForEach(douyuMessages, (message) =>
             {
                 if (message == null) { return; }
                 List<Action<AbstractDouyuMessage>> handlers = null;

                 if (m_TypeToHandler.TryGetValue(message.type, out handlers))
                 {
                     handlers.ForEach((handler) =>
                     {
                         handler(message);
                     });
                 }
             });
        }

        public BarrageClient AddHandler(string type,Action<AbstractDouyuMessage> handler)
        {
            lock (m_locker)
            {
                List<Action<AbstractDouyuMessage>> handlers = null;
                if (m_TypeToHandler.TryGetValue(type, out handlers))
                {
                    handlers.Add(handler);
                }
                else
                {
                    handlers = new List<Action<AbstractDouyuMessage>>();
                    handlers.Add(handler);
                    m_TypeToHandler[type] = handlers;
                }
                
            }
            return this;
        }

        public BarrageClient RemoveHandler(string type,Action<AbstractDouyuMessage> handler)
        {
            lock (m_locker)
            {
                List<Action<AbstractDouyuMessage>> handlers = null;
                if (m_TypeToHandler.TryGetValue(type, out handlers))
                {
                    handlers.Remove(handler);
                }
            }
            return this;
        }

        internal static object WaitImpl(Task<object> task, int timeout)
        {
            if (task.IsFaulted)
            {
                Exception ex = task.Exception;
                var aex = ex as AggregateException;
                if (aex != null && aex.InnerExceptions.Count == 1)
                {
                    ex = aex.InnerExceptions[0];
                }
                throw ex;
            }
            if (!task.IsCompleted)
            {
                try
                {
                    if (!task.Wait(timeout)) throw new TimeoutException();
                }
                catch (AggregateException aex)
                {
                    if (aex.InnerExceptions.Count == 1) throw aex.InnerExceptions[0];
                    throw;
                }
            }
            return task.Result;
        }
    }
}
