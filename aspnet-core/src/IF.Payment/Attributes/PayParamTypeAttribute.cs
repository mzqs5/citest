using System;

namespace IF.Payment.Attributes
{
    public class PayParamTypeAttribute : Attribute
    {
        public string AppIdKey { get; set; }

        public string MerchantNoKey { get; set; }

        public string PaySecretKey { get; set; }

        public string JsonKey { get; set; }
    }
}
