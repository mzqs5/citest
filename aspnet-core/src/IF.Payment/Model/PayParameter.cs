namespace IF.Payment.Model
{
    public class PayParameter
    {
        /// <summary>
        /// 商户号
        /// </summary>
        public string merchant_no { get; set; }
        /// <summary>
        /// 秘钥
        /// </summary>
        public string pay_secret { get; set; }
        /// <summary>
        /// appid
        /// </summary>
        public string appid { get; set; }
        /// <summary>
        /// 其他参数JSON数据
        /// </summary>
        public string json { get; set; }
    }

    
}
