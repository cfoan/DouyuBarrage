using Douyu.Messages;
using System;
using System.Text;

namespace Douyu
{
    /// <summary>
    /// 斗鱼消息处理
    /// </summary>
    public class BarrageConsole
    {

        public static Func<string, string> GiftNameResolver { get; set; } = (x) => GiftUtil.GiftName(x);

        /// <summary>
        /// 控制台打印，如果传入的参数为null,则不打印
        /// </summary>
        /// <param name="douyuMessage"></param>
        public static void PrintBarrage(AbstractDouyuMessage douyuMessage)
        {
            if (douyuMessage == null)
                return; 

            ShowBarrageViewInternal(douyuMessage);
        }

        public static void PrintBarrage(AbstractDouyuMessage[] douyuMessages)
        {
            if (douyuMessages == null)
                return; 

            ShowBarrageViewInternal(douyuMessages);
        }

        private static void ShowBarrageViewInternal(params AbstractDouyuMessage[] douyuMessages)
        {
            if (douyuMessages == null)
                return;

            StringBuilder sb = new StringBuilder();
            Array.ForEach(douyuMessages, (msg) =>
             {
                 var msgView = MessageToString(msg);
                 if (!string.IsNullOrWhiteSpace(msgView))
                     sb.AppendLine(msgView);
             });
            System.Console.Write(sb.ToString());
        }

        internal static string MessageToString(AbstractDouyuMessage douyuMessage)
        {
            switch (douyuMessage.type)
            {
                case BarrageConstants.TYPE_BARRAGE:
                    return $"[弹幕]{((Barrage)douyuMessage).nn}:{((Barrage)douyuMessage).txt}";
                case BarrageConstants.TYPE_GIFT:
                    var gift = douyuMessage as Gift;
                    var name = GiftNameResolver(gift.gfid);
                    return $"[礼物]来自{gift.nn} [{name}] {(gift.hits == null ? "" : $"{gift.hits}连击")}";
                case BarrageConstants.TYPE_SUPER_BARRAGE:
                    return $"[超级弹幕]{((SuperBarrage)douyuMessage).content}";
                default:
                    break;
            }
            return "";
        }

    }
}
