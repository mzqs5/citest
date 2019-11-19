using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IF.Payment.Attributes;

namespace IF.Payment.Model
{
    public enum PayParamType
    {
        [PayParamTypeAttribute(AppIdKey = "WeChatPayAppID", PaySecretKey = "WeChatPayKey", MerchantNoKey = "WeChatPayMerchantNo", JsonKey = "WXPayJson")]
        WeChatPay,
        [PayParamTypeAttribute(AppIdKey = "AliPayAppID", PaySecretKey = "AliPayKey", MerchantNoKey = "AliPayMerchantNo", JsonKey = "AliPayJson")]
        AliPay
    }
}
