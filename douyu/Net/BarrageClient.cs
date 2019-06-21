using Douyu.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        private const int MillisecondPerTick = 5 * 1000;

        private readonly Timer m_timer;
        private readonly object m_locker;
        private readonly BarrageConnection m_client;
        private readonly ConcurrentDictionary<string, List<Action<AbstractDouyuMessage>>> m_TypeToHandler;

        private int m_tick = 0;
        private string m_roomId;
        private bool closed = false;
        private TaskCompletionSource<LoginResponse> m_login;

        public BarrageClient(string roomId)
        {
            m_locker = new object();
            m_timer = new Timer(HeatbeatOrReconnect, null, Timeout.Infinite, Timeout.Infinite);
            m_TypeToHandler = new ConcurrentDictionary<string, List<Action<AbstractDouyuMessage>>>();
            m_client = new BarrageConnection();
            m_client.OnDataArrive += m_client_OnDataArrive;
            m_client.OnConnectToServer += m_client_OnConnectToServer;
            m_client.OnDisconnectFromServer += m_client_OnDisconnectFromServer;
            m_roomId = roomId;
            Handle(BarrageConstants.TYPE_LOGIN_RESPONSE).Add((message) => m_login?.TrySetResult((LoginResponse)message));
        }

        public string RoomId => m_roomId;
        public bool Active => m_client.Connected;

        public BarrageClient Connect()
        {
            if (!m_client.Connected)
            {
                m_client.ConnectToServer();

                Send(new LoginRequest());
                if (m_login == null || m_login.Task.IsCompleted)
                {
                    m_login = new TaskCompletionSource<LoginResponse>();
                }
                try
                {
                    WaitImpl(m_login.Task, LoginTimeout);
                }
                catch (TimeoutException)
                {
                    m_client.Disconnect();
                    throw;
                }
                m_timer.Change(0, MillisecondPerTick);
                EnterRoom(m_roomId);
            }
            return this;
        }

        private BarrageClient EnterRoom(string roomId, string groupId = "-9999")
        {
            if (string.IsNullOrWhiteSpace(roomId))
                throw new ArgumentNullException("roomId");

            Debug.WriteLine($"try enter room {roomId}...");

            m_roomId = roomId;
            JoinGroup joinGroup = new JoinGroup();
            joinGroup.gid = groupId;
            joinGroup.rid = roomId;
            Send(joinGroup);
            return this;
        }

        private void HeatbeatOrReconnect(object sender)
        {
            if (closed)
            {
                return;
            }

            if (this.m_client.Connected)
            {
                if (++m_tick >= 9)
                {
                    var keepAlive = new Keepalive()
                    {
                        tick = (Utils.CurrentTimestampUtc() / 1000).ToString() //取的秒数
                    };
                    Send(keepAlive);
                    m_tick = 0;
                }
            }
            else
            {
                Debug.WriteLine("重连");
                try
                {
                    Connect();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        public void Close()
        {
            lock (this)
            {
                if (!closed)
                {
                    closed = true;
                    if (m_client != null)
                    {
                        m_client.OnDataArrive -= m_client_OnDataArrive;
                        m_client.OnConnectToServer -= m_client_OnConnectToServer;
                        m_client.OnDisconnectFromServer -= m_client_OnDisconnectFromServer;
                        m_client.Disconnect();
                    }
                    m_timer.Change(Timeout.Infinite, Timeout.Infinite);
                    m_timer.Dispose();
                }
            }
        }

        private void m_client_OnDisconnectFromServer()
        {
            
        }

        private void m_client_OnConnectToServer()
        {
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
            if (m_client.Connected)
            {
                m_client.Send(Converters.Instance.Encode(message));
            }
        }

        internal static object WaitImpl<T>(Task<T> task, int timeout)
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
