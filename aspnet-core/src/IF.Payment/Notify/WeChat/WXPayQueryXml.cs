using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IF.Payment.Notify
{
    [System.Xml.Serialization.XmlType(TypeName = "xml")]
    /// <summary>
    /// 预支付查询实体
    /// </summary>
    public class WXPayQueryXml
    {
        /// <summary>
        /// 微信appid
        /// </summary>
        public string appid { get; set; }
        /// <summary>
        /// 微信支付商户号
        /// </summary>
        public string mch_id { get; set; }
        /// <summary>
        /// 随机字符串
        /// </summary>
        public string nonce_str { get; set; }
        /// <summary>
        /// 微信交易id
        /// </summary>
        public string transaction_id { get; set; }
        /// <summary>
        /// 签名
        /// </summary>
        public string sign { get; set; }

    }
}
