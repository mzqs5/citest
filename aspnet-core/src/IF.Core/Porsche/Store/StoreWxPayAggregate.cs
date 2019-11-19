using IF.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IF.Porsche
{
    [Table("StoreWxPay")]
    public class StoreWxPayAggregate : AggregateRootBase<int>
    {
        /// <summary>
        /// 经销商Id
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// 公众账号ID
        /// </summary>
        public string appid { get; set; }

        /// <summary>
        /// 应用密钥
        /// </summary>
        public string secret { get; set; }

        /// <summary>
        /// 商户号
        /// </summary>
        public string mch_id { get; set; }

        /// <summary>
        /// 微信支付API密钥
        /// </summary>
        public string key { get; set; }

    }
}
