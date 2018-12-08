using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Linq;
using Douyu.Messages;
using System.Collections.Concurrent;
using System.Threading;

namespace Douyu.Net
{
    /// <summary>
    /// 弹幕客户端
    /// </summary>
    public class BarrageConnection
    {
        public event Action OnConnectToServer;
        public event Action OnDisconnectFromServer;
        public event Action<Packet[]> OnDataArrive;

        private const int RequestMessageType= 689;
        private const int ResponseMessageType= 690;
        private const int ReceiveBufferSize = 8192 * 2;
        private const int SendBufferSize = 4096;
        private const byte PacketSplitor = 0x0;

        private const int ConnectTimeOut = 5000;
        private const string DouyuDomain = "openbarrage.douyutv.com";//"danmu.douyutv.com"
        private readonly int[] DouyuPorts = new int[] { 8061, 8062, 12601, 12602 };
        
        private byte[] m_receiveBuffer;
        private Socket m_socket;
        private volatile bool m_isConnected;

        public bool Connected => m_isConnected;

        public BarrageConnection()
        {
            m_receiveBuffer = new byte[ReceiveBufferSize];
        }

        /// <summary>
        /// 接收弹幕客户端的启动
        /// </summary>
        /// <param name="eventHandler">收到的消息的处理</param>
        /// <returns>自己</returns>
        public BarrageConnection ConnectToServer()
        {
            if (m_isConnected) { return this; }
            var ips = new IPAddress[0];
            try
            {
                ips = Dns.GetHostAddresses(DouyuDomain);
            }
            catch (Exception ex)
            {
#if DEBUG
                System.Console.WriteLine(ex.StackTrace);
#endif
            }
            foreach (var ip in ips)
            {
                foreach (var port in DouyuPorts)
                {
                    Socket tmp = null;
                    try
                    {
                        tmp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        var asyncState = tmp.BeginConnect(new IPEndPoint(ip, port), null, null);

                        if (asyncState.AsyncWaitHandle.WaitOne(ConnectTimeOut, true))
                        {
                            tmp.EndConnect(asyncState); // checks for exception

                            if (m_isConnected = tmp.Connected)
                            {
                                m_socket = tmp;
                                ThreadPool.QueueUserWorkItem((state) =>
                                {
                                    OnConnectToServer?.Invoke();
                                }, null);
                                break;
                            }

                        }
                        else
                        {
                            tmp.Close();
                            tmp.Dispose();
                        }

                    }
                    catch (Exception)
                    {
                        if (tmp != null && !tmp.Connected)
                        {
                            tmp.Close();
                            tmp.Dispose();
                        }
                    }
                }
                if (m_isConnected) { break; }
            }

            if (!m_isConnected) { throw new Exception("Unable to connect to server"); }
            m_socket.BeginReceive(m_receiveBuffer, 0, m_receiveBuffer.Length, SocketFlags.None, ReceiveComplted, m_socket);
            return this;
        }

        /// <summary>
        /// 停止接收弹幕服务器消息
        /// </summary>
        public void Close()
        {
            if (m_socket != null)
            {
                try
                {
                    m_socket.Close();
                    m_socket.Dispose();
                }
                catch
                {
                    /**
                     * do nothing
                     * */
                }
            }
            if (m_isConnected)
            {
                m_isConnected = false;
                bytesUnhandledLastTime = 0;
                ThreadPool.QueueUserWorkItem((state) =>
                {
                    OnDisconnectFromServer?.Invoke();
                }, null);
            }
        }

        public void Send(string data)
        {
            Packet packet = new Packet();
            packet.Data = data;
            packet.Type = RequestMessageType;
            SendPacketInternal(packet);
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
                    int handledBytes = 0;
                    var total = bytesTransferredThisTime + bytesUnhandledLastTime;
                    var pkts = ReadPackets(m_receiveBuffer, 0, total, out handledBytes);
                    Buffer.BlockCopy(m_receiveBuffer, handledBytes, m_receiveBuffer, 0, total - handledBytes);
                    bytesUnhandledLastTime = total - handledBytes;
                    var writeIndex = bytesUnhandledLastTime;
                    ThreadPool.QueueUserWorkItem((state) =>
                    {
                        OnDataArrive?.Invoke(pkts.ToArray());
                    }, null);
                    
                    socket.BeginReceive(m_receiveBuffer, writeIndex, m_receiveBuffer.Length - writeIndex, SocketFlags.None, ReceiveComplted, socket);
                }
                else if (bytesTransferredThisTime <= 0)
                {
                    Close();
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
                System.Console.WriteLine($"{ex.Message},{ex.StackTrace}");
#endif
                Close();
            }
        }

        private void ExpandBuffer(ref byte[]buffer)
        {
            var oldBufferLength = buffer.Length;
            var newBuffer = new byte[oldBufferLength * 2];
            Buffer.BlockCopy(buffer, 0, newBuffer, 0, oldBufferLength);
            buffer = newBuffer;
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
            var count = state.Count;
            var bytesTransferred = socket.EndSend(ia);
            if (bytesTransferred < count)
            {
                DoSend(socket, buffer, bytesTransferred, count - bytesTransferred);
            }
        }

        /// <summary>
        /// 组包和发送
        /// </summary>
        /// <param name="packet"></param>
        private void SendPacketInternal(Packet packet)
        {
            byte[] buffer = new byte[SendBufferSize];
            var totalBytes = WritePacket(packet, ref buffer);
            DoSend(m_socket, buffer, 0, totalBytes);
        }

        /// <summary>
        /// 发送字符数组
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        private void DoSend(Socket socket,byte[] buffer, int offset, int count)
        {
            try
            {
                socket.BeginSend(buffer, offset, count, SocketFlags.None, SendComplted, new SendAsyncState() { Socket = socket, Offset = offset, Buffer = buffer, Count = count });
            }
            catch (Exception ex)
            {
                Close();
#if DEBUG
                System.Console.WriteLine(ex.Message);
#endif
                throw ex;
            }
        }

        /// <summary>
        /// 包写入字符数组
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns>总共写入的字符数量</returns>
        private int WritePacket(Packet packet,ref byte[]buffer)
        {
            int writeOffset = 0, writeCount = 0;
            writeOffset += 8;
            writeCount += 8;

            BitConverter.GetBytes(packet.Type).CopyTo(buffer, writeOffset);
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
                pkt.Type = BitConverter.ToInt32(buffer, readIndex);
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
    /// 发送异步对象
    /// </summary>
    class SendAsyncState
    {
        public Socket Socket { get; set; }

        public byte[] Buffer { get; set; }

        public int Offset { get; set; }

        public int Count { get; set; }

        public long Seq { get; set; }
    }
}
