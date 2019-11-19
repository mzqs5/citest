using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace IF.Payment.Notify
{
    /// <summary>
    /// 微信支付 通知结果类 实体解析
    /// <para>详细地址：https://pay.weixin.qq.com/wiki/doc/api/jsapi.php?chapter=9_7</para>
    /// </summary>

    [System.Xml.Serialization.XmlType(TypeName = "xml")]
    public class WXPayNotifyResult
    {
        /// <summary>
        /// <para>说明：返回状态码</para>
        /// <para>示例值：SUCCESS/FAIL</para>
        /// <para>描述：此字段是通信标识，非交易标识，交易是否成功需要查看result_code来判断</para>
        /// </summary>
        public string return_code { get; set; }

        /// <summary>
        /// <para>说明：返回信息</para>
        /// <para>示例值：签名失败</para>
        /// <para>描述：返回信息，如非空，为错误原因等等...</para>
        /// </summary>
        public string return_msg { get; set; }

        /// <summary>
        /// <para>说明：公众账号ID</para>
        /// <para>示例值：wx8888888888888888</para>
        /// <para>描述：微信分配的公众账号ID（企业号corpid即为此appId）</para>
        /// </summary>
        public string appid { get; set; }

        /// <summary>
        /// <para>说明：商户号</para>
        /// <para>描述：微信支付分配的商户号</para>
        /// </summary>
        public string mch_id { get; set; }

        /// <summary>
        /// <para>说明：设备号</para>
        /// <para>描述：微信支付分配的终端设备号</para>
        /// </summary>
        public string device_info { get; set; }

        /// <summary>
        /// <para>说明：随机字符串</para>
        /// <para>描述：随机字符串，不长于32位</para>
        /// </summary>
        public string nonce_str { get; set; }

        /// <summary>
        /// 签名
        /// </summary>
        public string sign { get; set; }

        /// <summary>
        /// 签名类型
        /// </summary>
        public string sign_type { get; set; }

        /// <summary>
        /// <para>说明：业务结果</para>
        /// <para>示例值：SUCCESS/FAIL</para>
        /// </summary>
        public string result_code { get; set; }

        /// <summary>
        /// 错误代码
        /// </summary>
        public string err_code { get; set; }

        /// <summary>
        /// 错误代码描述
        /// </summary>
        public string err_code_des { get; set; }

        /// <summary>
        /// 用户标识
        /// </summary>
        public string openid { get; set; }

        /// <summary>
        /// 是否关注公众账号 Y-关注，N-未关注
        /// </summary>
        public string is_subscribe { get; set; }

        /// <summary>
        /// 交易类型
        /// </summary>
        public string trade_type { get; set; }

        /// <summary>
        /// 付款银行
        /// </summary>
        public string bank_type { get; set; }

        /// <summary>
        /// 订单金额 /单位分 主要
        /// </summary>
        public int total_fee { get; set; }

        /// <summary>
        /// 应结订单金额
        /// </summary>
        public int settlement_total_fee { get; set; }

        /// <summary>
        /// 货币种类
        /// </summary>
        public string fee_type { get; set; }

        /// <summary>
        /// 现金支付金额
        /// </summary>
        public string cash_fee { get; set; }

        /// <summary>
        /// 现金支付货币类型
        /// <para>必有：否</para>
        /// </summary>
        public string cash_fee_type { get; set; }

        /// <summary>
        /// 总代金券金额
        /// <para>必有：否</para>
        /// </summary>
        public string coupon_fee { get; set; }

        /// <summary>
        /// 代金券使用数量
        /// <para>必有：否</para>
        /// </summary>
        public string coupon_count { get; set; }

        /// <summary>
        /// 微信支付订单号
        /// <para>必有：是</para>
        /// </summary>
        public string transaction_id { get; set; }

        /// <summary>
        /// 商户订单号
        /// <para>必有：是</para>
        /// </summary>
        public string out_trade_no { get; set; }

        /// <summary>
        /// 支付状态
        /// </summary>
        public string trade_state { get; set; }

        /// <summary>
        /// 商家数据包
        /// <para>必有：否</para>
        /// <para>统一下单有，该包必有</para>
        /// </summary>
        public string attach { get; set; }

        /// <summary>
        /// 支付完成时间
        /// <para>必有：否</para>
        /// <para>示例：20141030133525</para>
        /// </summary>
        public string time_end { get; set; }
    }

}