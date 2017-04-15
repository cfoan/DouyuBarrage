using Douyu.Messages;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Text;

namespace Douyu
{
    /// <summary>
    /// 斗鱼消息处理
    /// </summary>
    public class DouyuBarrage
    {
        private static string logPath = AppDomain.CurrentDomain.BaseDirectory + "\\log.txt";
        private static object locker = new object();

        private static DouyuBarrage instance = new DouyuBarrage();

        public static DouyuBarrage Instance
        {
            get { return instance; }
        }

        private DouyuBarrage() { }

        /// <summary>
        /// 控制台打印，如果传入的参数为null,则不打印
        /// </summary>
        /// <param name="douyuMessage"></param>
        public void ShowBarrageView(AbstractDouyuMessage douyuMessage)
        {
            if (douyuMessage == null) { return; }
            ShowBarrageViewInternal(douyuMessage);
        }

        public void ShowBarrageView(AbstractDouyuMessage[] douyuMessages)
        {
            if (douyuMessages == null) { return; }
            ShowBarrageViewInternal(douyuMessages);
        }

        private void ShowBarrageViewInternal(params AbstractDouyuMessage[]douyuMessages)
        {
            if (douyuMessages == null) { return; }
            StringBuilder sb = new StringBuilder();
            Array.ForEach(douyuMessages, (msg) =>
             {
                 var msgView = MessageView(msg);
                 if (!string.IsNullOrWhiteSpace(msgView))
                 {
                     sb.AppendLine(msgView);
                 }
             });
            Console.Write(sb.ToString());
        }

        internal string MessageView(AbstractDouyuMessage douyuMessage)
        {
            switch (douyuMessage.type)
            {
                case BarrageConstants.TYPE_BARRAGE:
                    var chatMsg = douyuMessage as Barrage;
                    return string.Format("[弹幕]{0}：{1}", chatMsg.nn, chatMsg.txt);
                case BarrageConstants.TYPE_GIFT:
                    var gift = douyuMessage as Gift;
                    if (GiftUtil.GiftName(gift.gfid) == "unknown")
                    {
                        Dumps(gift.raw);
                    }
                    var giftInfo = string.Format("【{0}】 {1}", GiftUtil.GiftName(gift.gfid), !string.IsNullOrWhiteSpace(gift.hits) ?
                        string.Format("{0}连击", gift.hits) : "");
                    return string.Format("[礼物]来自{0} {1}", gift.nn, giftInfo);
                case BarrageConstants.TYPE_SUPER_BARRAGE:
                    var superBarrage = douyuMessage as SuperBarrage;
                    return string.Format("[超级弹幕]{0}", superBarrage.content);
                default:
                    break;
            }
            return "";
        }

        public static void Dumps(AbstractDouyuMessage douyuMessage)
        {
            if (douyuMessage == null) { return; }
            Dumps(douyuMessage.ToString());
        }

        public static void Dumps(string log)
        {
            lock (locker)
            {
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(logPath, true))
                {
                    sw.WriteLine(log);
                }
            }
        }

    }
}
