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
            //White55开解说
            var roomId=Utils.GetRoomId(new Uri("https://www.douyu.com/wt55kai"));
            if (string.IsNullOrWhiteSpace(roomId))
            {
                Console.WriteLine("获取房间号失败");
                return;
            }
            var client = new DanmuClient();
            client.OnNewBulletScreen += Client_OnNewBulletScreen;
            if (client.Start())
            {
                client.EnterRoom(roomId);
            }
            Console.ReadKey();

        }

        private static void Client_OnNewBulletScreen(object obj)
        {
            var pkts = obj as List<Packet>;
            pkts.ForEach((pkt) =>
            {
                var msg = DanmuParser.Parse(pkt.Data);
                var danmu = DanmuParser.ToString(msg);
                if (danmu != null && danmu.Length > 0)
                {
                    DanmuParser.Dumps(danmu);
                    Console.WriteLine(danmu);
                }
            });
        }
    }
}
