using System;

namespace IF.Payment.Attributes
{
    public class PayTypeAttribute : Attribute
    {
        /// <summary>
        /// 支付第三方代码
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 命名空间
        /// </summary>
        public string NameSpaces { get; set; }
        /// <summary>
        /// 支付类名称
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// 支付第三方名称
        /// </summary>
        public string DisplayText { get; set; }
    }
}
