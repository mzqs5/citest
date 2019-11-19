using IF.Payment.Abstracts;
using IF.Payment.Model;

namespace IF.Payment.Concrete.WeChatPay.Model
{
    public class WXQuery : QueryBase
    {

        public string Money { get; set; }
        public string ProductName { get; set; }
        public string NotifyUrl { get; set; }
        /// <summary>
        /// 支付成功后的返回路径
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// 取消支付时返回的路径
        /// </summary>
        public string Cancelurl { get; set; }
        public string OpenId { get; set; }

        internal string Mch_id => PayConfig.GetMerchantNo(PayParamType.WeChatPay);
        internal string AppId => PayConfig.GetAppId(PayParamType.WeChatPay);
        internal string Appkey => PayConfig.GetPaySecret(PayParamType.WeChatPay);
    }
}
