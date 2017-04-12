using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DouyuDanmu.Messages
{
    /// <summary>
    /// 弹幕消息
    /// </summary>
    public class Barrage : AbstractDouyuMessage
    {
        /// <summary>
        /// 弹幕租id
        /// </summary>
        public string gid { get; set; }

        /// <summary>
        /// 房间id
        /// </summary>
        public string rid { get; set; }

        /// <summary>
        /// 发送者id
        /// </summary>
        public string uid { get; set; }

        /// <summary>
        /// 发送者昵称
        /// </summary>
        public string nn { get; set; }

        /// <summary>
        /// 弹幕内容
        /// </summary>
        public string txt { get; set; }

        /// <summary>
        /// 弹幕唯一id
        /// </summary>
        public string cid { get; set; }

        /// <summary>
        /// 用户等级
        /// </summary>
        public string level { get; set; }

        /// <summary>
        /// 礼物头衔：默认值0（表示没有头衔）
        /// </summary>
        public string gt { get; set; }

        /// <summary>
        /// 颜色：默认值0（表示默认颜色弹幕）
        /// </summary>
        public string col { get; set; }

        /// <summary>
        /// 客户端类型：默认值0（表示web用户）
        /// </summary>
        public string ct { get; set; }

        /// <summary>
        /// 房间权限组：默认值1（表示普通权限用户）
        /// </summary>
        public string rg { get; set; }

        /// <summary>
        /// 平台权限组：默认值1（表示普通权限用户）
        /// </summary>
        public string pg { get; set; }

        /// <summary>
        /// 酬勤等级：默认值0（表示没有酬勤）
        /// </summary>
        public string dlv { get; set; }

        /// <summary>
        /// 酬勤数量：默认值0（表示没有酬勤数量）
        /// </summary>
        public string dc { get; set; }

        /// <summary>
        /// 最高酬勤等级：默认值0（表示全站都没有酬勤）
        /// </summary>
        public string bdlv { get; set; }

        public override string type
        {
            get
            {
                return "chatmsg";
            }
        }
    }
}
