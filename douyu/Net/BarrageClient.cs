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
        private const int LoginTimeout = 5000;
        private const int KeepAliveTimeout = 45 * 1000;

        private readonly Timer m_timer;
        private readonly object m_locker;
        private readonly BarrageConnection m_client;
        private readonly ConcurrentDictionary<string, List<Action<AbstractDouyuMessage>>> m_TypeToHandler;

        private string m_roomId;
        private TaskCompletionSource<object> loginTcs;
        private Action m_connectHandler;
        private Action m_disconnectHandler;

        
        
        public BarrageClient()
        {
            m_locker = new object();
            m_timer = new Timer(Heatbeat, null, Timeout.Infinite, Timeout.Infinite);
            m_TypeToHandler = new ConcurrentDictionary<string, List<Action<AbstractDouyuMessage>>>();
            m_client = new BarrageConnection();
            m_client.OnDataArrive += m_client_OnDataArrive;
            m_client.OnConnectToServer += m_client_OnConnectToServer;
            m_client.OnDisconnectFromServer += m_client_OnDisconnectFromServer;


            Handle(BarrageConstants.TYPE_LOGIN_RESPONSE).Add((message) => LoginResponseHandler?.Invoke((LoginResponse)message));
        }

        public string RoomId { get { return m_roomId; } }

        Action<LoginResponse> LoginResponseHandler { get; set; }

        public BarrageClient Connect()
        {
            m_client.ConnectToServer();

            Send(new LoginRequest());
            loginTcs = new TaskCompletionSource<object>();
            LoginResponseHandler = (message) => loginTcs.SetResult(message);
            WaitImpl(loginTcs.Task, LoginTimeout);
            m_timer.Change(0, KeepAliveTimeout);
            return this;
        }

        public BarrageClient EnterRoom(string roomId, string groupId = "-9999")
        {
            if (string.IsNullOrWhiteSpace(roomId))
                throw new ArgumentNullException("roomId");
#if DEBUG
            System.Console.WriteLine("try enter room {0}...", roomId);
#endif
            m_roomId = roomId;
            JoinGroup joinGroup = new JoinGroup();
            joinGroup.gid = groupId;
            joinGroup.rid = roomId;
            Send(joinGroup);
            return this;
        }

        private void Heatbeat(object sender)
        {
            var keepAlive = new Keepalive()
            {
                tick = (Utils.CurrentTimestampUtc() / 1000).ToString() //取的秒数
            };
            Send(keepAlive);
        }

        public void Stop()
        {
            m_client.Close();
            m_timer.Change(Timeout.Infinite, Timeout.Infinite);
            m_timer.Dispose();
        }

        public BarrageClient ConnectHandler(Action handler)
        {
            m_connectHandler = handler;
            return this;
        }

        public BarrageClient DisconnectHandler(Action handler)
        {
            m_disconnectHandler = handler;
            return this;
        }

        private void m_client_OnDisconnectFromServer()
        {
            m_timer.Change(Timeout.Infinite, Timeout.Infinite);
            m_disconnectHandler?.Invoke();
        }

        private void m_client_OnConnectToServer()
        {
            m_connectHandler?.Invoke();
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
                     handlers?.ForEach((handler) =>
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

        public List<Action<AbstractDouyuMessage>> Handle(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                throw new ArgumentNullException("type");
            }
            lock (m_locker)
            {
                List<Action<AbstractDouyuMessage>> handlers = null;
                m_TypeToHandler.TryGetValue(type, out handlers);
                if (handlers == null)
                {
                    handlers = new List<Action<AbstractDouyuMessage>>();
                    m_TypeToHandler[type] = handlers;
                }
                return handlers;

            }
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
            if (!m_client.Connected)
            {
                throw new BarrageClientNotConnectedToServerException();
            }

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

        public class BarrageClientNotConnectedToServerException : Exception { }
    }
}
