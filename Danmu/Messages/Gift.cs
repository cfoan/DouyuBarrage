using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DouyuDanmu.Messages
{
    /// <summary>
    /// 赠送礼物消息
    /// </summary>
    public class Gift : AbstractDouyuMessage
    {
        /// <summary>
        /// 房间id
        /// </summary>
        public string rid { get; set; }

        /// <summary>
        /// 弹幕分组id
        /// </summary>
        public string gid { get; set; }

        /// <summary>
        /// 礼物id
        /// </summary>
        public string gfid { get; set; }

        /// <summary>
        /// 礼物显示样式
        /// </summary>
        public string gs { get; set; }

        /// <summary>
        /// 用户id
        /// </summary>
        public string uid { get; set; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        public string nn { get; set; }

        /// <summary>
        /// 用户战斗力
        /// </summary>
        public string str { get; set; }

        /// <summary>
        /// 用户等级
        /// </summary>
        public string level { get; set; }

        /// <summary>
        /// 用户体重
        /// </summary>
        public string dw { get; set; }

        /// <summary>
        /// 礼物个数，默认值1（表示1个礼物）
        /// </summary>
        public string gfcnt { get; set; }

        /// <summary>
        /// 礼物连击次数：默认值 1（表示 1 连击）
        /// </summary>
        public string hits { get; set; }

        /// <summary>
        /// 酬勤头衔：默认值 0（表示没有酬勤）
        /// </summary>
        public string dlv { get; set; }

        /// <summary>
        /// 酬勤个数：默认值 0（表示没有酬勤数量）
        /// </summary>
        public string dc { get; set; }

        /// <summary>
        /// 全站最高酬勤等级：默认值 0（表示全站都没有酬勤）
        /// </summary>
        public string bdl { get; set; }

        /// <summary>
        /// 房间身份组：默认值 1（表示普通权限用户）
        /// </summary>
        public string rg { get; set; }

        /// <summary>
        /// 平台身份组：默认值 1（表示普通权限用户）
        /// </summary>
        public string pg { get; set; }

        /// <summary>
        /// 红包 id：默认值 0（表示没有红包）
        /// </summary>
        public string rpid { get; set; }

        /// <summary>
        /// 红包开启剩余时间：默认值 0（表示没有红包）
        /// </summary>
        public string slt { get; set; }

        /// <summary>
        /// 红包销毁剩余时间：默认值 0（表示没有红包）
        /// </summary>
        public string elt { get; set; }

        public override string type
        {
            get
            {
                return "dgb";
            }
        }
    }
}
