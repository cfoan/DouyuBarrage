using Douyu.Messages;
using Douyu.Net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Douyu
{
    class Program
    {
        static void Main(string[] args)
        {
            List<BarrageClient> clients = new List<BarrageClient>(8);
            /**
             * https://www.douyu.com/chenyifaer 陈一发儿
             * https://www.douyu.com/wt55kai 卢本伟五五开White
             * https://www.douyu.com/chenyifaer|https://www.douyu.com/wt55kai
             * https://www.douyu.com/t/KPL|https://www.douyu.com/606118
             * 67373|138286
             * */
            System.Console.WriteLine("接下来要做什么(输入对应的数字) \r\n1.输入房间号进入房间\r\n2.输入url进入房间 \r\n3.exit");
            var key = System.Console.ReadLine();

            if (key.Length >= 0)
            {
                List<string> roomIds = new List<string>(8);
                switch ((int)key[0])
                {
                    case 49:
                        System.Console.WriteLine("输入要进入的房间号");
                        var read1 = System.Console.ReadLine();
                        var roomIds1 = read1.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                        var tr = 0;
                        foreach (var roomId in roomIds1)
                        {
                            if (!int.TryParse(roomId, out tr))
                            {
                                continue;
                            }
                            roomIds.Add(roomId);
                        }
                        if (roomIds.Count == 0)
                        {
                            System.Console.WriteLine("can't find any valid roomid,press any key to exit");
                            break;
                        }
                        break;
                    case 50:
                        System.Console.WriteLine("输入要进入的房间网址");
                        var read2 = System.Console.ReadLine();
                        var urls = read2.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var url in urls)
                        {
                            try
                            {
                                var t = Utils.GetRoomId2(new Uri(url));
                                if (string.IsNullOrWhiteSpace(t))
                                {
                                    System.Console.WriteLine($"获取房间号失败,url:{url}");
                                    continue;
                                }
                                roomIds.Add(t);
                            }
                            catch
                            {
                                continue;
                            }
                        }
                        if (roomIds.Count == 0)
                        {
                            System.Console.WriteLine("can't recognize any room,press any key to exit");
                            break;
                        }

                        break;
                    default:
                        break;
                }

                roomIds.ForEach((roomId) =>
                {
                    var client = new BarrageClient();
                    client.Connect()
                        .AddHandler((message) => BarrageConsole.PrintBarrage(message), new string[] { BarrageConstants.TYPE_SUPER_BARRAGE, BarrageConstants.TYPE_GIFT, BarrageConstants.TYPE_BARRAGE })
                        .EnterRoom(roomId);
                    clients.Add(client);
                });
            }
            System.Console.ReadKey();
            clients.ForEach((client) =>
            {
                client.Stop();
            });

        }
    }
}
