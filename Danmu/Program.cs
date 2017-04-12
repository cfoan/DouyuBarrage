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
            var roomId = Utils.GetRoomId(new Uri("https://www.douyu.com/chenyifaer"));
            if (string.IsNullOrWhiteSpace(roomId))
            {
                Console.WriteLine("获取房间号失败");
                return;
            }
            new BarrageClient().Start(HandleBulletScreenClientEvent).EnterRoom(roomId);
            Console.ReadKey();

        }

        private static void HandleBulletScreenClientEvent(object sender, BarrageEventArgs e)
        {
            if (e.Action == ActionType.PacketArrive)
            {
                e.PacketsReceived?.ForEach((pkt) =>
                {
                    DouyuBarrage.instance.ConsoleLog(pkt.Data);
                });
            }
        }
    }
}
