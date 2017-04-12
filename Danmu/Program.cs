using DouyuDanmu.Messages;
using DouyuDanmu.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DouyuDanmu
{
    class Program
    {
        static void Main(string[] args)
        {
            //var roomId=Utils.GetRoomId(new Uri("https://www.douyu.com/chenyifaer"));
            //if (string.IsNullOrWhiteSpace(roomId))
            //{
            //    Console.WriteLine("获取房间号失败");
            //    return;
            //}
            //new BarrageClient().Start(HandleBulletScreenClientEvent).EnterRoom(roomId);
            DouyuMessage m = new DouyuMessage();
            m.LoadAllDouyuMessage();
            m.Parse();
            Console.ReadKey();

        }

        private static void HandleBulletScreenClientEvent(object sender, BarrageEventArgs e)
        {
            if (e.Action == ActionType.PacketArrive)
            {
                e.PacketsReceived?.ForEach((pkt) =>
                {
                    if (pkt.Data.IndexOf("chatmsg") ==1)
                    {
                        var msg=BarrageConverter.Default.ParseString(pkt.Data);
                        //var msg = DanmuParser.Parse(pkt.Data);
                        var danmu = DouyuMessage.ToString(msg);
                        if (danmu != null && danmu.Length > 0)
                        {
                            //DanmuParser.Dumps(danmu);
                            Console.WriteLine(danmu);
                        }
                    }
                });
            }
        }
    }
}
