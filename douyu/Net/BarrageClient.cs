using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Douyu.Net
{
    /// <summary>
    /// client动作类型
    /// </summary>
    public enum ActionType
    {
        Connect,Disconnect,PacketArrive
    }

    /// <summary>
    /// 弹幕客户端
    /// </summary>
    public class BarrageClient
    {
        public event EventHandler<BarrageEventArgs> OnClientEvent;

        private const int RequestMessageType= 689;
        private const int ResponseMessageType= 690;
        private const int ReceiveBufferSize = 8192;
        private const int SendBufferSize = 4096;
        private const int LoginTimeout = 5000;
        private const int ConnectTimeOut = 2000;
        private const string DouyuDomain = "danmu.douyutv.com";
        private readonly int[] DouyuPorts = new int[] { 8061, 8062, 12601, 12602 };
        
        private byte[] receiveBuffer;
        private Socket socket;
        private System.Timers.Timer timer;
        private volatile bool isConnected;
        private volatile bool isLogined;

        public BarrageClient()
        {
            timer = new System.Timers.Timer();
            receiveBuffer = new byte[ReceiveBufferSize];
        }

        /// <summary>
        /// 接收弹幕客户端的启动
        /// </summary>
        /// <param name="eventHandler">收到的消息的处理</param>
        /// <returns>自己</returns>
        public BarrageClient Start(EventHandler<BarrageEventArgs> eventHandler=null)
        {
            Stop();

            var ips = new IPAddress[0];
            try
            {
                ips = Dns.GetHostAddresses(DouyuDomain);
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.StackTrace);
#endif
            }
            foreach (var ip in ips)
            {
                foreach (var port in DouyuPorts)
                {
                    try
                    {
                        var tmp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                        var asyncState = tmp.BeginConnect(new IPEndPoint(ip, port), null, null);

                        if (asyncState.AsyncWaitHandle.WaitOne(ConnectTimeOut, true))
                        {
                            tmp.EndConnect(asyncState); // checks for exception

                            if (isConnected = tmp.Connected)
                            {
                                socket = tmp;
                                BarrageEventArgs eventArgs = new BarrageEventArgs();
                                eventArgs.Action = ActionType.Connect;
                                OnClientEvent?.Invoke(this, eventArgs);
                                break;
                            }

                        }
                        else
                        {
                            tmp.Close();
                            tmp.Dispose();
                            throw new TimeoutException("Unable to connect to endpoint");
                        }

                    }
                    catch (Exception ex)
                    {
                        /**
                         * do nothing
                         * */
                    }
                }
                if (isConnected) { break; }
            }

            if (!isConnected) { throw new Exception("Unable to connect to server"); }
            socket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, ReceiveComplted, socket);
            OnClientEvent += eventHandler;
            timer.Elapsed += Heatbeat;
            timer.Interval = 45 * 1000;
            timer.Start();
            Login();
            return this;
        }

        /// <summary>
        /// 进入房间
        /// </summary>
        /// <param name="roomId"></param>
        public void EnterRoom(string roomId)
        {
#if DEBUG
            Console.WriteLine("try enter room {0}...", roomId);
#endif
            Packet packet = new Packet();
            packet.Data = string.Format("type@=joingroup/rid@={0}/gid@=-9999/", roomId);
            packet.Flag = RequestMessageType;
            SendPacketInternal(packet);
        }

        /// <summary>
        /// 停止接收弹幕服务器消息
        /// </summary>
        public void Stop()
        {
            if (socket != null)
            {
                try
                {
                    socket.Close();
                    socket.Dispose();
                }
                catch
                {
                    /**
                     * do nothing
                     * */
                }
            }
            if (isConnected)
            {
                isConnected = false;
                isLogined = false;
                bytesUnhandledLastTime = 0;
                BarrageEventArgs eventArgs = new BarrageEventArgs();
                eventArgs.Action = ActionType.Disconnect;
                OnClientEvent?.Invoke(this, eventArgs);
            }
            timer.Stop();
            timer.Elapsed -= Heatbeat;
        }

        private TaskCompletionSource<object> login;

        /// <summary>
        /// 发送登录请求并等待回复
        /// </summary>
        private void Login()
        {
            login = new TaskCompletionSource<object>();
            Packet packet = new Packet();
            packet.Data = "type@=loginreq/";
            packet.Flag = RequestMessageType;
            SendPacketInternal(packet);
            try
            {
                var loginSuccess=(bool)WaitImpl(login.Task, LoginTimeout);
                if (!loginSuccess) throw new Exception("Unable to login to server");
            }
            catch(Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.StackTrace);
#endif
                Stop();
                throw;
            }
        }

        /// <summary>
        /// 发送心跳包
        /// </summary>
        private void KeepAlive()
        {
            Packet pkt = new Packet();
            pkt.Data = string.Format("type@=keeplive/tick@={0}/", Utils.CurrentTimestampUtc() / 1000);//取的秒数
            pkt.Flag = RequestMessageType;
            SendPacketInternal(pkt);
        }

        private void Heatbeat(object sender, ElapsedEventArgs e)
        {
            if (socket != null && socket.Connected)
            {
                KeepAlive();
            }
        }

        /// <summary>
        /// 接收到服务器包的处理
        /// </summary>
        /// <param name="packets"></param>
        private void OnPacketsReceived(List<Packet> packets)
        {
            if (packets.Count == 0) { return; }
            if (!isLogined)
            {
                var loginSuccess = packets[0].Data.IndexOf("loginres") != -1;
                login.TrySetResult(loginSuccess);
                if (!loginSuccess) { return; }
                isLogined = true;
                packets.RemoveAt(0);
            }
            BarrageEventArgs eventArgs = new BarrageEventArgs();
            eventArgs.Action = ActionType.PacketArrive;
            eventArgs.PacketsReceived = packets;
            OnClientEvent?.Invoke(this, eventArgs);
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

        #region handle send and receive events

        private volatile int bytesUnhandledLastTime = 0;

        /// <summary>
        /// 处理接收完成事件
        /// </summary>
        /// <param name="ia"></param>
        private void ReceiveComplted(IAsyncResult ia)
        {
            try
            {
                var socket = ia.AsyncState as Socket;
                var bytesTransferredThisTime = socket.EndReceive(ia);
                if (bytesTransferredThisTime > 0)
                {
                    if (bytesTransferredThisTime == receiveBuffer.Length || bytesTransferredThisTime + bytesUnhandledLastTime == receiveBuffer.Length)//buffer may be not big enough
                    {
                        var oldBufferLength = receiveBuffer.Length;
                        var newBuffer = new byte[oldBufferLength * 2];
                        var newBufferWriteIndex = bytesTransferredThisTime + bytesUnhandledLastTime;
                        Buffer.BlockCopy(receiveBuffer, 0, newBuffer, 0, receiveBuffer.Length);
                        receiveBuffer = newBuffer;
                        var countNewBufferCanWrite = newBuffer.Length - oldBufferLength;
                        bytesUnhandledLastTime = oldBufferLength;
                        socket.BeginReceive(receiveBuffer, newBufferWriteIndex, countNewBufferCanWrite, SocketFlags.None, ReceiveComplted, socket);
                    }
                    else
                    {
                        int handledBytes = 0;
                        var total = bytesTransferredThisTime + bytesUnhandledLastTime;
                        var pkts = ReadPackets(receiveBuffer, 0, total, out handledBytes);
                        Buffer.BlockCopy(receiveBuffer, handledBytes, receiveBuffer, 0, total - handledBytes);
                        bytesUnhandledLastTime = total - handledBytes;
                        var writeIndex = bytesUnhandledLastTime;
                        var countCanWrite = receiveBuffer.Length - writeIndex;
                        OnPacketsReceived(pkts);
                        socket.BeginReceive(receiveBuffer, writeIndex, countCanWrite, SocketFlags.None, ReceiveComplted, socket);
                    }
                }
                else if (bytesTransferredThisTime <= 0)
                {
                    Stop();
                }
            }
            catch (ObjectDisposedException)
            {
                /**
                 * lololol
                 **/
            }
            catch (SocketException ex)
            {
#if DEBUG
                Console.WriteLine(ex.StackTrace);
#endif
                Stop();
            }
        }

        /// <summary>
        /// 处理发送完成事件
        /// </summary>
        /// <param name="ia"></param>
        private void SendComplted(IAsyncResult ia)
        {
            var state = ia.AsyncState as SendAsyncState;
            var socket = state.Socket;
            var buffer = state.Buffer;
            var bytesTransferred = socket.EndSend(ia);
            if (bytesTransferred < state.Count)
            {
                DoSend(buffer, bytesTransferred, state.Count - bytesTransferred);
            }
        }

        /// <summary>
        /// 组包和发送
        /// </summary>
        /// <param name="packet"></param>
        private void SendPacketInternal(Packet packet)
        {
            byte[] buffer = new byte[SendBufferSize];
            var totalBytes = WritePacket(packet, ref buffer, 0, buffer.Length);
            DoSend(buffer, 0, totalBytes);
        }

        /// <summary>
        /// 发送字符数组
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        private void DoSend(byte[] buffer, int offset, int count)
        {
            try
            {
                socket.BeginSend(buffer, offset, count, SocketFlags.None, SendComplted, new SendAsyncState() { Socket = socket, Offset = offset, Buffer = buffer, Count = count });
            }
            catch (Exception ex)
            {
                Stop();
#if DEBUG
                Console.WriteLine(ex.Message);
#endif
            }
        }

        /// <summary>
        /// 包写入字符数组
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns>总共写入的字符数量</returns>
        private int WritePacket(Packet packet,ref byte[]buffer,int offset,int count)
        {
            int writeOffset = offset, writeCount = 0;
            writeOffset += 8;
            writeCount += 8;

            BitConverter.GetBytes(packet.Flag).CopyTo(buffer, writeOffset);
            writeOffset += 4;
            writeCount += 4;

            var dataBytes = Encoding.UTF8.GetBytes(packet.Data, 0, packet.Data.Length, buffer, writeOffset);
            writeOffset += dataBytes;
            writeCount += dataBytes;

            buffer[writeOffset++] = 0x0;
            writeCount++;

            packet.LengthA = writeCount - 4;
            packet.LengthB = writeCount - 4;
            BitConverter.GetBytes(packet.LengthA).CopyTo(buffer, 0);
            BitConverter.GetBytes(packet.LengthB).CopyTo(buffer, 4);
            return writeCount;
        }

        /// <summary>
        /// 从字节数字读出包
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="handledBytes">处理的字节总数</param>
        /// <returns>读出的包列表</returns>
        private List<Packet> ReadPackets(byte[] buffer, int offset, int count, out int handledBytes)
        {
            List<Packet> packets = new List<Packet>(4);
            int readIndex = offset;
            handledBytes = 0;
            for (;;)
            {
                if (count <= 12) { break; }

                var pkt = new Packet();
                pkt.LengthA = BitConverter.ToInt32(buffer, readIndex);
                readIndex += 4; count -= 4;
                pkt.LengthB = BitConverter.ToInt32(buffer, readIndex);
                readIndex += 4; count -= 4;
                pkt.Flag = BitConverter.ToInt32(buffer, readIndex);
                readIndex += 4; count -= 4;
                if (pkt.LengthA <= 0) { throw new Exception("bad protocol"); }
                var dataBytes = pkt.LengthA - 8 - 1;

                if (count - 1 < dataBytes) { break; }
                pkt.Data = Encoding.UTF8.GetString(buffer, readIndex, dataBytes);
                readIndex += dataBytes; count -= dataBytes;

                if (buffer[readIndex++] != 0) { throw new Exception("bad protocol"); }
                count--;
                packets.Add(pkt);
                handledBytes = readIndex;
            }

            return packets;
        }
        #endregion
    }

    /// <summary>
    /// 弹幕客户端事件对象
    /// </summary>
    public class BarrageEventArgs : EventArgs
    {
        public ActionType Action { get; set; }

        public List<Packet> PacketsReceived { get; set; }

        public object UserToken { get; set; }
    }

    /// <summary>
    /// 发送异步对象
    /// </summary>
    internal class SendAsyncState
    {
        public Socket Socket { get; set; }

        public byte[] Buffer { get; set; }

        public int Offset { get; set; }

        public int Count { get; set; }
    }
}
