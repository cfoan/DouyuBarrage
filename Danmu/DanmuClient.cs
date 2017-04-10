using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace DouyuDanmu
{
    internal class AsyncState
    {
        public Socket Socket { get; set; }

        public byte[] Buffer { get; set; }

        public int Offset { get; set; }

        public int Count { get; set; }
    }

    public class DanmuClient
    {
        private const int REQUEST_MESSAGETYPE= 689;
        private const int RESPONSE_MESSAGETYPE= 690;
        private const int BUFFER_SIZE = 8192;
        private const int LOGIN_TIMEOUT = 2000;
        private const int CONNECT_TIMEOUT = 2000;
        private const string DouyuDanmuDomain = "danmu.douyutv.com";
        private readonly int[] DouyuDanmuPorts = new int[] { 8061, 8062, 12601, 12602 };
        
        private byte[] buffer;
        private Socket socket;
        private Timer timer;
        private volatile bool isConnected;

        public event Action<object> OnNewBulletScreen;
        public event Action<DanmuClient> OnConnected;
        public event Action<DanmuClient> OnDisconnected;

        public DanmuClient()
        {
            timer = new Timer();
            buffer = new byte[BUFFER_SIZE];
        }

        public bool Start()
        {
            Stop();

            var ips = new IPAddress[0];
            try
            {
                ips = Dns.GetHostAddresses(DouyuDanmuDomain);
            }
            catch (Exception ex)
            {
#if DEBUG
                Console.WriteLine(ex.StackTrace);
#endif
            }
            foreach (var ip in ips)
            {
                foreach (var port in DouyuDanmuPorts)
                {
                    try
                    {
                        var tmp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                        var asyncState = tmp.BeginConnect(new IPEndPoint(ip, port), null, null);

                        if (asyncState.AsyncWaitHandle.WaitOne(CONNECT_TIMEOUT, true))
                        {
                            tmp.EndConnect(asyncState); // checks for exception

                            if (isConnected = tmp.Connected)
                            {
                                socket = tmp;
                                OnConnected?.Invoke(this);
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


            if (isConnected)
            {
#if DEBUG
                Console.WriteLine("已连接到斗鱼弹幕服务器,{0}", socket.RemoteEndPoint.ToString());
#endif
                socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, ReceiveComplted, socket);

                timer.Elapsed += Heatbeat;
                timer.Interval = 45 * 1000;
                timer.Start();

                Login();
            }
            return isConnected;
        }

        public void EnterRoom(string roomId)
        {
            Console.WriteLine("enter room {0}...", roomId);
            Packet pkt = new Packet();
            pkt.Data = string.Format("type@=joingroup/rid@={0}/gid@=-9999/", roomId);
            pkt.Type = REQUEST_MESSAGETYPE;
            var data = AssembleRequest(pkt);
            SendPacket(data.Item1, 0, data.Item2);
        }

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
                OnDisconnected?.Invoke(this);
            }
            timer.Stop();
            timer.Elapsed -= Heatbeat;
        }

        private void Login()
        {
            Packet packet = new Packet();
            packet.Data = "type@=loginreq/";
            packet.Type = REQUEST_MESSAGETYPE;
            var data = AssembleRequest(packet);
            SendPacket(data.Item1, 0, data.Item2);
        }

        private void KeepAlive()
        {
            Packet pkt = new Packet();
            pkt.Data = string.Format("type@=keeplive/tick@={0}/", Utils.CurrentTimestampUtc() / 1000);//取的秒数
            pkt.Type = REQUEST_MESSAGETYPE;
            var data = AssembleRequest(pkt);
            SendPacket(data.Item1, 0, data.Item2);
        }

        private void Heatbeat(object sender, ElapsedEventArgs e)
        {
            if (socket != null && socket.Connected)
            {
                KeepAlive();
            }
        }

        #region transfer,assemble and parse

        private volatile int bytesUnhandledLastTime = 0;
        private void ReceiveComplted(IAsyncResult ia)
        {
            try
            {
                var socket = ia.AsyncState as Socket;
                var bytesTransferredThisTime = socket.EndReceive(ia);
                if (bytesTransferredThisTime > 0)
                {
                    if (bytesTransferredThisTime == buffer.Length || bytesTransferredThisTime + bytesUnhandledLastTime == buffer.Length)//buffer may be not big enough
                    {
#if DEBUG
                        Console.WriteLine("buffer not big ennough");
#endif
                        var oldBufferLength = buffer.Length;
                        var newBuffer = new byte[oldBufferLength * 2];
                        var newBufferWriteIndex = bytesTransferredThisTime + bytesUnhandledLastTime;
                        Buffer.BlockCopy(buffer, 0, newBuffer, 0, buffer.Length);
                        buffer = newBuffer;
                        var countNewBufferCanWrite = newBuffer.Length - oldBufferLength;
                        bytesUnhandledLastTime = oldBufferLength;
                        socket.BeginReceive(buffer, newBufferWriteIndex, countNewBufferCanWrite, SocketFlags.None, ReceiveComplted, socket);
                    }
                    else
                    {
                        int handledBytes = 0;
                        var total = bytesTransferredThisTime + bytesUnhandledLastTime;
                        var pkts = ReadPackets(buffer, 0, total, out handledBytes);
                        Buffer.BlockCopy(buffer, handledBytes, buffer, 0, total - handledBytes);
                        bytesUnhandledLastTime = total - handledBytes;
                        var writeIndex = bytesUnhandledLastTime;
                        var countCanWrite = buffer.Length - writeIndex;
                        OnNewBulletScreen?.Invoke(pkts);
                        socket.BeginReceive(buffer, writeIndex, countCanWrite, SocketFlags.None, ReceiveComplted, socket);
                    }
                }
                else if (bytesTransferredThisTime <= 0)
                {
                    Stop();
                }
            }
            catch (SocketException ex)
            {
#if DEBUG
                Console.WriteLine(ex.StackTrace);
#endif
                Stop();
            }
        }

        private void SendComplted(IAsyncResult ia)
        {
            var state = ia.AsyncState as AsyncState;
            var socket = state.Socket;
            var buffer = state.Buffer;
            var bytesTransferred = socket.EndSend(ia);
            if (bytesTransferred < state.Count)
            {
                SendPacket(buffer, bytesTransferred, state.Count - bytesTransferred);
            }
        }

        private void SendPacket(byte[] buffer, int offset, int count)
        {
            try
            {
                socket.BeginSend(buffer, offset, count, SocketFlags.None, SendComplted, new AsyncState() { Socket = socket, Offset = 0, Buffer = buffer, Count = count });
            }
            catch (Exception ex)
            {
                Stop();
#if DEBUG
                Console.WriteLine(ex.Message);
#endif
            }
        }

        private Tuple<byte[],int> AssembleRequest(Packet packet)
        {
            byte[] buffer = new byte[4096];

            int writeOffset = 0, count = 0;
            writeOffset += 8;
            count += 8;

            BitConverter.GetBytes(packet.Type).CopyTo(buffer, writeOffset);
            writeOffset += 4;
            count += 4;

            var dataBytes = Encoding.UTF8.GetBytes(packet.Data, 0, packet.Data.Length, buffer, writeOffset);
            writeOffset += dataBytes;
            count += dataBytes;

            buffer[writeOffset++] = 0x0;
            count++;

            packet.LengthA = count - 4;
            packet.LengthB = count - 4;
            BitConverter.GetBytes(packet.LengthA).CopyTo(buffer, 0);
            BitConverter.GetBytes(packet.LengthB).CopyTo(buffer, 4);
            return new Tuple<byte[], int>(buffer, count);
        }

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
}
