using DouyuDanmu.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DouyuDanmu
{
    class Program
    {
        static void Main(string[] args)
        {
            var roomId=Utils.GetRoomId(new Uri("https://www.douyu.com/chenyifaer"));
            if (string.IsNullOrWhiteSpace(roomId))
            {
                Console.WriteLine("获取房间号失败");
                return;
            }
            new BulletScreenClient().Start(HandleBulletScreenClientEvent).EnterRoom(roomId);

            Console.ReadKey();

        }

        private static void HandleBulletScreenClientEvent(object sender, BulletScreenEventArgs e)
        {
            if (e.Action == ActionType.PacketArrive)
            {
                e.PacketsReceived?.ForEach((pkt) =>
                {
                    if (pkt.Data.IndexOf("chatmsg") == -1)
                    {
                        var msg = DanmuParser.Parse(pkt.Data);
                        var danmu = DanmuParser.ToString(msg);
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
