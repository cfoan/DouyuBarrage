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
        
        private const int LoginTimeout = 5000;
        private const int KeepAliveTimeout = 45 * 1000;

        private Timer m_timer;
        private object m_locker;
        private string m_curRoomId;
        private TaskCompletionSource<object> loginTcs;
        private readonly BarrageConnection m_client;
        private readonly Action<LoginResponse> m_onLoginResponse;
        private readonly List<Func<AbstractDouyuMessage, bool>> m_filters;
        private readonly ConcurrentDictionary<string, List<Action<AbstractDouyuMessage>>> m_TypeToHandler;

        public string RoomId { get { return m_curRoomId; } }

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

            m_onLoginResponse=(message)=> loginTcs?.SetResult(true);
            AddHandler((message) =>m_onLoginResponse?.Invoke((LoginResponse)message), BarrageConstants.TYPE_LOGIN_RESPONSE);
        }

        public BarrageClient Connect()
        {
            m_client.ConnectToServer();
            WaitForLogin();
            return this;
        }

        private void WaitForLogin()
        {
            Send(new LoginRequest());
            loginTcs = new TaskCompletionSource<object>();
            WaitImpl(loginTcs.Task, LoginTimeout);
        }

        public BarrageClient EnterRoom(string roomId, string groupId = "-9999")
        {
            if (string.IsNullOrWhiteSpace(roomId))
                throw new ArgumentNullException("roomId");
#if DEBUG
            System.Console.WriteLine("try enter room {0}...", roomId);
#endif
            m_curRoomId = roomId;
            JoinGroup joinGroup = new JoinGroup();
            joinGroup.gid = groupId;
            joinGroup.rid = roomId;
            Send(joinGroup);
            return this;
        }

        private void Heatbeat(object sender)
        {
            var keepAlive = new Keepalive();
            keepAlive.tick=(Utils.CurrentTimestampUtc() / 1000).ToString();//取的秒数
            Send(keepAlive);
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

        public BarrageClient AddHandler(Action<AbstractDouyuMessage> handler, string type)
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

        public BarrageClient AddHandler(Action<AbstractDouyuMessage> handler,params string[] barrageTypeNames)
        {
            Array.ForEach(barrageTypeNames, (typeName) =>
             {
                 AddHandler(handler, typeName);
             });
            return this;
        }

        public BarrageClient RemoveHandler(Action<AbstractDouyuMessage> handler,string type)
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

        internal void Send(AbstractDouyuMessage message)
        {
            m_client.Send(Converters.Instance.Encode(message));
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
