using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Douyu.Messages
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

        public override string name
        {
            get
            {
                return "礼物";
            }
        }

        public override string ToString()
        {
            var giftInfo = string.Format("【{0}】 {1}", GiftUtil.GiftName(gfid), !string.IsNullOrWhiteSpace(hits) ?
                string.Format("{0}连击", hits) : "");
            return string.Format("[礼物]来自{0} {1}", nn, giftInfo);
        }
    }

    internal class GiftUtil
    {
        public static string GiftName(string id)
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
                case "268":
                    return "发财";
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
                case "712":
                    return "棒棒哒";
                case "714":
                    return "怂";
                case "713":
                    return "辣眼睛";
                default:
                    return "unknown";
            }
        }
    }
}
