using IF.Payment.Abstracts;
using IF.Payment.Model;

namespace IF.Payment.Concrete.WeChatPay.Model
{
    public class WXSMQuery : QueryBase
    {
        public string Money { get; set; }
        public string ProductName { get; set; }
        public string NotifyUrl { get; set; }
        public string Productid { get; set; }


        internal string Mch_id => PayConfig.GetMerchantNo(PayParamType.WeChatPay);
        internal string AppId => PayConfig.GetAppId(PayParamType.WeChatPay);
        internal string Appkey => PayConfig.GetPaySecret(PayParamType.WeChatPay);
    }
}
