using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DouyuDanmu
{
    public class DanmuMessage
    {
        /// <summary>
        /// 弹幕id
        /// </summary>
        public string DanmuId { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 房间号
        /// </summary>
        public string RoomId { get; set; }
        
        /// <summary>
        /// 用户id
        /// </summary>
        public string Uid { get; set; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 房间身份组
        /// </summary>
        public string Rg { get; set; }

        public string Icon { get; set; }

        /// <summary>
        /// 用户等级
        /// </summary>
        public string Level { get; set; }

        /// <summary>
        /// 弹幕内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 连击
        /// </summary>
        public string Hits { get; set; }

        /// <summary>
        /// 礼物id
        /// </summary>
        public string GiftId { get; set; }

        public string Raw { get; set; }
    }

    public class DanmuParser
    {
        private static string logPath = AppDomain.CurrentDomain.BaseDirectory + "\\log.txt";
        static object locker = new object();

        public const string KEY_CID = "cid";
        public const string KEY_TYPE = "type";
        public const string KEY_ROOM_ID = "rid";
        public const string KEY_UID = "uid";
        public const string KEY_NICKNAME = "nn";
        public const string KEY_TXT = "txt";
        public const string KEY_ICON = "ic";
        public const string KEY_LEVEL = "level";
        public const string KEY_GIFT_ID= "gfid";
        public const string KEY_HITS = "hits";

        public static DanmuMessage Parse(string data)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            var differentParts = data.Split(new string[] { "@=", "/" }, StringSplitOptions.None);

            for (int i = 0; i < differentParts.Length / 2; i++)
            {
                var key = differentParts[2 * i].Replace("@S", "/").Replace("@A", "@");
                var value = differentParts[2 * i + 1].Replace("@S", "/").Replace("@A", "@");
                dict[key] = value;
            }
            DanmuMessage msg = new DanmuMessage();
            msg.Type = dict.ContainsKey(KEY_TYPE) ? dict[KEY_TYPE] : "";
            msg.Content = dict.ContainsKey(KEY_TXT) ? dict[KEY_TXT] : "";
            msg.NickName = dict.ContainsKey(KEY_NICKNAME) ? dict[KEY_NICKNAME] : "";
            msg.Uid = dict.ContainsKey(KEY_UID) ? dict[KEY_UID] : "";
            msg.RoomId = dict.ContainsKey(KEY_ROOM_ID) ? dict[KEY_ROOM_ID] : "";
            msg.Level = dict.ContainsKey(KEY_LEVEL) ? dict[KEY_LEVEL] : "";
            msg.GiftId= dict.ContainsKey(KEY_GIFT_ID) ? dict[KEY_GIFT_ID] : "";
            msg.Hits = dict.ContainsKey(KEY_HITS) ? dict[KEY_HITS] : "";
            msg.DanmuId= dict.ContainsKey(KEY_CID) ? dict[KEY_CID] : "";
            msg.Raw = data;
            return msg;
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

        internal static string GiftName(string id)
        {
            switch (id)
            {
                case "124":
                    return "电竞三丑";
                case "191":
                    return "100鱼丸";
                case "192":
                    return "赞";
                case "193":
                    return "弱鸡";
                case "194":
                    return "666";
                case "195":
                    return "飞机";
                case "196":
                    return "火箭";
                case "380":
                    return "好人卡";
                case "479":
                    return "帐篷";
                case "519":
                    return "呵呵";
                case "520":
                    return "稳";
                case "530":
                    return "天秀";
                default:
                    return "unknown";
            }
        }

        public static string ToString(DanmuMessage message)
        {
            switch (message.Type)
            {
                case "chatmsg":
                    return string.Format("[弹幕]{0}：{1}",message.NickName, message.Content);
                case "dgb":
                    //if (GiftName(message.GiftId) == "unknown")
                    //{
                    //    Dumps(message.Raw);
                    //}
                    var giftInfo=string.Format("【{0}】 {1}", GiftName(message.GiftId), !string.IsNullOrWhiteSpace(message.Hits) ? string.Format("{0}连击", message.Hits) : "");
                    return string.Format("[礼物]来自{0} {1}", message.NickName, giftInfo);
                default:
                    return "";
            }
        }

    }
}
