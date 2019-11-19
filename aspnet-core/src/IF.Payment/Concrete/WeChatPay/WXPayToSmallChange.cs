using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using IF.Payment.Common;

namespace IF.Payment.Concrete.WeChatPay
{
    public class WXPayToSmallChange
    {
        public static ResultModel PayToSmallChange(PayToSmallChangeModel pay)
        {
            ResultModel result = new ResultModel();
            /*1.生成签名*/
            SortedDictionary<string, object> dics = new SortedDictionary<string, object>();
            dics.Add("mch_appid", PayConfig.GetAppId(Payment.Model.PayParamType.WeChatPay));
            dics.Add("mchid", PayConfig.GetMerchantNo(Payment.Model.PayParamType.WeChatPay));
            dics.Add("partner_trade_no", pay.trade_no);
            dics.Add("nonce_str", Utils.getNoncestr());
            dics.Add("openid", pay.openid);
            dics.Add("check_name", "NO_CHECK");
            dics.Add("desc", "提现");
            dics.Add("amount", Convert.ToInt64(decimal.Parse(pay.amount) * 100));
            dics.Add("spbill_create_ip", PayHelp.GetUserIP());
            dics.Add("sign", Utils.CreateSign(dics, PayConfig.GetPaySecret(Payment.Model.PayParamType.WeChatPay)));
            /*2.生成xml数据*/
            var sb = Utils.WeChatSignXml(dics);
            /*开始请求接口*/
            try
            {
                Log.log("PayLogger", sb);
                string xmlStr = Utils.WxCerHttpPost("https://api.mch.weixin.qq.com/mmpaymkttransfers/promotion/transfers", sb);
                if (!string.IsNullOrWhiteSpace(xmlStr))
                {
                    Log.log("PayLogger", xmlStr);
                    var returnXML = XmlUtil.XmlToObect<PayToSmallChangeResultModel>(xmlStr);
                    if (returnXML.return_code == "SUCCESS")
                    {
                        if (returnXML.result_code == "SUCCESS")
                        {
                            result.code = 1200;
                            result.msg = "成功";
                            return result;
                        }
                        result.code = 200;
                        result.msg = returnXML.err_code_des;
                        return result;
                    }
                    result.code = 200;
                    result.msg = returnXML.return_msg;
                    return result;
                }
                result.code = 200;
                result.msg = "接口返回空数据";
            }
            catch (Exception ex)
            {
                result.code = 200;
                result.msg = ex.Message;
                Log.log("PayLogger", ex.Message);
            }
            return result;
        }
    }

    public class ResultModel
    {
        public int code { get; set; }

        public string msg { get; set; }
    }
    [XmlType(TypeName = "xml")]
    public class PayToSmallChangeResultModel
    {
        public string return_code { get; set; }

        public string return_msg { get; set; }


        public string mch_appid { get; set; }

        public string mchid { get; set; }

        public string device_info { get; set; }

        public string nonce_str { get; set; }

        public string result_code { get; set; }

        public ErrCode err_code { get; set; }

        public string err_code_des { get; set; }

        public string partner_trade_no { get; set; }

        public string payment_no { get; set; }

        public string payment_time { get; set; }
    }

    public enum ErrCode
    {
        [Description("没有该接口权限")]
        NO_AUTH,
        [Description("金额超限")]
        AMOUNT_LIMIT,
        [Description("参数错误")]
        PARAM_ERROR,
        [Description("Openid错误")]
        OPENID_ERROR,
        [Description("付款错误")]
        SEND_FAILED,
        [Description("余额不足")]
        NOTENOUGH,
        [Description("系统繁忙，请稍后再试。")]
        SYSTEMERROR,
        [Description("姓名校验出错")]
        NAME_MISMATCH,
        [Description("签名错误")]
        SIGN_ERROR,
        [Description("Post内容出错")]
        XML_ERROR,
        [Description("两次请求参数不一致")]
        FATAL_ERROR,
        [Description("超过频率限制，请稍后再试。")]
        FREQ_LIMIT,
        [Description("已经达到今日付款总额上限/已达到付款给此用户额度上限")]
        MONEY_LIMIT,
        [Description("商户API证书校验出错")]
        CA_ERROR,
        [Description("无法给非实名用户付款")]
        V2_ACCOUNT_SIMPLE_BAN,
        [Description("请求参数中包含非utf8编码字符")]
        PARAM_IS_NOT_UTF8,
        [Description("该用户今日付款次数超过限制,如有需要请登录微信支付商户平台更改API安全配置")]
        SENDNUM_LIMIT
    }

    public class PayToSmallChangeModel
    {
        public string trade_no { get; set; }
        public string openid { get; set; }
        public string amount { get; set; }

    }
}
