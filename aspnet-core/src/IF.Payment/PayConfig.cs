using System.Configuration;
using IF.Payment.Attributes;
using IF.Payment.Common;
using IF.Payment.Model;

namespace IF.Payment
{
    public class PayConfig
    {

        public static string CerPath { get { return ConfigurationManager.AppSettings["CerPath"]; } }
        /// <summary>
        /// 获取AppId
        /// </summary>
        /// <param name="payParam"></param>
        /// <returns></returns>
        public static string GetAppId(PayParamType payParam)
        {
            var Config = EnumService.GetAttribute<PayParamTypeAttribute>(payParam);
            return ConfigurationManager.AppSettings[Config.AppIdKey];
        }
        /// <summary>
        /// 获取支付商户号
        /// </summary>
        /// <param name="payParam"></param>
        /// <returns></returns>
        public static string GetMerchantNo(PayParamType payParam)
        {
            var Config = EnumService.GetAttribute<PayParamTypeAttribute>(payParam);
            return ConfigurationManager.AppSettings[Config.MerchantNoKey];
        }
        /// <summary>
        /// 获取支付密钥
        /// </summary>
        /// <param name="payParam"></param>
        /// <returns></returns>
        public static string GetPaySecret(PayParamType payParam)
        {
            var Config = EnumService.GetAttribute<PayParamTypeAttribute>(payParam);
            return ConfigurationManager.AppSettings[Config.PaySecretKey];
        }
        /// <summary>
        /// 获取支付扩展参数
        /// </summary>
        /// <param name="payParam"></param>
        /// <returns></returns>
        public static string GetJson(PayParamType payParam)
        {
            var Config = EnumService.GetAttribute<PayParamTypeAttribute>(payParam);
            return ConfigurationManager.AppSettings[Config.JsonKey];
        }
    }
}
