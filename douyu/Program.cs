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
             * 67373|138286
             * */
            Console.WriteLine("接下来要做什么(输入对应的数字) \r\n1.输入房间号进入房间\r\n2.输入url进入房间 \r\n3.exit");
            var key = Console.ReadLine();
            if (key.Length >= 0)
            {
                switch ((int)key[0])
                {
                    case 49:
                        Console.WriteLine("输入要进入的房间号");
                        var read1 = Console.ReadLine();
                        var roomIds1 = read1.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                        var validRoomIds1 = new string[roomIds1.Length];
                        var index1 = 0; var tr = 0;
                        foreach (var roomId in roomIds1)
                        {
                            if (!int.TryParse(roomId, out tr))
                            {
                                continue;
                            }
                            validRoomIds1[index1++] = roomId;
                        }
                        if (index1 == 0)
                        {
                            Console.WriteLine("can't find any valid roomid,press any key to exit");
                            break;
                        }
                        Array.ForEach(validRoomIds1.Take(index1).ToArray(), (roomId) =>
                         {
                             clients.Add(new BarrageClient().Start(HandleBarrageClientEvent).EnterRoom(roomId));
                         });
                        break;
                    case 50:
                        Console.WriteLine("输入要进入的房间网址");
                        var read2 = Console.ReadLine();
                        var urls = read2.Split(new char[] { '|' },StringSplitOptions.RemoveEmptyEntries);
                        var validRoomIds2 = new string[urls.Length];
                        var index = 0;
                        foreach (var url in urls)
                        {
                            try
                            {
                                var t = Utils.GetRoomId2(new Uri(url));
                                if (string.IsNullOrWhiteSpace(t))
                                {
                                    Console.WriteLine("获取房间号失败");
                                    continue;
                                }
                                validRoomIds2[index++] = t;
                            }
                            catch
                            {
                                continue;
                            }
                        }
                        if (index == 0)
                        {
                            Console.WriteLine("can't recognize any room,press any key to exit");
                            break;
                        }
                        Array.ForEach(validRoomIds2.Take(index).ToArray(), (roomId) =>
                         {
                             clients.Add(new BarrageClient().Start(HandleBarrageClientEvent).EnterRoom(roomId));
                         });
                        break;
                    default:
                        break;

                }
            }
            Console.ReadKey();
            clients.ForEach((client) =>
            {
                client.Stop();
            });

        }

        private static void HandleBarrageClientEvent(object sender, BarrageEventArgs e)
        {
            if (e.Action == ActionType.NewMessage)
            {
                DouyuBarrage.Instance.ShowBarrageView(e.Messages.ToArray());
            }
        }
    }
}
