using IF.Payment.Abstracts;
using IF.Payment.Model;

namespace IF.Payment.Concrete.AliPay.Model
{
    public class AliQuery : QueryBase
    {
        public string Money { get; set; }
        public string ProductName { get; set; }
        public string NotifyUrl { get; set; }
        public string ReturnUrl { get; set; }

        /// <summary>
        /// 合作者身份ID
        /// </summary>
        internal string Partner => PayConfig.GetMerchantNo(PayParamType.AliPay);
        /// <summary>
        /// 合作者身份ID
        /// </summary>
        internal string Seller_id => PayConfig.GetMerchantNo(PayParamType.AliPay);
        /// <summary>
        /// 秘钥
        /// </summary>
        internal string Key => PayConfig.GetPaySecret(PayParamType.AliPay);

    }
}
