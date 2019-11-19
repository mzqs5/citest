using IF.Payment.Attributes;

namespace IF.Payment.Model
{
    public enum PayType
    {
        [PayTypeAttribute(DisplayText = "微信公众号支付", Code = "wxgzhpay", NameSpaces = "IF.Payment.Concrete.WeChatPay", ClassName = "WXPay")]
        微信公众号支付,
        [PayTypeAttribute(DisplayText = "微信H5支付", Code = "wxh5pay", NameSpaces = "IF.Payment.Concrete.WeChatPay", ClassName = "WXH5Pay")]
        微信H5支付,
        [PayTypeAttribute(DisplayText = "微信扫码支付", Code = "wxsmpay", NameSpaces = "IF.Payment.Concrete.WeChatPay", ClassName = "WXSMPay")]
        微信扫码支付,
        [PayTypeAttribute(DisplayText = "支付宝支付", Code = "alipay", NameSpaces = "IF.Payment.Concrete.AliPay", ClassName = "AliPay")]
        支付宝支付,
        [PayTypeAttribute(DisplayText = "支付宝扫码支付", Code = "alismpay", NameSpaces = "IF.Payment.Concrete.AliPay", ClassName = "AliSMPay")]
        支付宝扫码支付,
        [PayTypeAttribute(DisplayText = "微信App支付", Code = "wxapppay", NameSpaces = "IF.Payment.Concrete.WeChatPay", ClassName = "WXAppPay")]
        微信App支付,
        [PayTypeAttribute(DisplayText = "支付宝App支付", Code = "aliapppay", NameSpaces = "IF.Payment.Concrete.AliPay", ClassName = "AliAppPay")]
        支付宝App支付
    }
}