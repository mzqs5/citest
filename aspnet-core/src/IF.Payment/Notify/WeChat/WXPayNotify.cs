using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using IF.Payment.Common;
using IF.Payment.Model;

namespace IF.Payment.Notify
{
    public class WXPayNotify
    {
        /// <summary>
        /// 微信支付通知
        /// </summary>
        /// <returns></returns>
        public static string Notify(Func<string, string, bool> func)
        {
            string POST = GetPostData();
            Log.log("PayLogger", "支付信息内容:" + POST);
            var notifyResult = XmlUtil.XmlToObect<WXPayNotifyResult>(POST);
            var isok = false;
            if (notifyResult != null)
            {
                try
                {
                    var nonce_str = DateTime.Now.ToString("yyyyMMddhhmmss");
                    //Log.log("PayLogger", "--------开始支付验证处理---------");
                    //支付验证处理
                    SortedDictionary<string, object> sodict = new SortedDictionary<string, object>();
                    sodict.Add("appid", notifyResult.appid);
                    sodict.Add("mch_id", notifyResult.mch_id);
                    sodict.Add("nonce_str", nonce_str);
                    sodict.Add("transaction_id", notifyResult.transaction_id);

                    WXPayQueryXml query = notifyResult.MapTo<WXPayNotifyResult, WXPayQueryXml>();
                    query.nonce_str = nonce_str;
                    query.sign = CreateSign(sodict, PayConfig.GetPaySecret(PayParamType.WeChatPay));

                    //Log.log("PayLogger", "预支付请求参数：" + JsonUtil.ToJson(query));
                    string XML = XmlUtil.SinClassToXml(query);
                    //Log.log("PayLogger", "请求xml数据：" + "<xml>" + XML + "</xml>");
                    string Value = Utils.HttpPost("https://api.mch.weixin.qq.com/pay/orderquery", "<xml>" + XML + "</xml>");
                    //Log.log("PayLogger", "请求orderquery回调数据：" + Value);
                    var result = XmlUtil.XmlToObect<WXPayNotifyResult>(Value);
                    //Log.log("PayLogger", "预支付请求结果：" + JsonUtil.ToJson(result));

                    //验证通过
                    if (result.trade_state.Trim().ToUpper() == "SUCCESS")
                    {
                        //Log.log("PayLogger", "------------验证成功----------------");
                        //商户系统内部订单号
                        string out_trade_no = result.out_trade_no.Trim();
                        //微信支付订单号
                        string trade_no = result.transaction_id.Trim();

                        isok = func(out_trade_no, trade_no);
                    }
                }
                catch (Exception e)
                {
                    Log.log("PayLogger", "解析支付信息错误：" + e.ToString());
                }
            }
            if (isok)
            {
                WXPayModelBasic Success = new WXPayModelBasic();
                Success.return_code = XmlUtil.XmlCATA("SUCCESS");
                Success.return_msg = XmlUtil.XmlCATA("OK");
                return XmlUtil.SinClassToXml(Success);
            }
            else
            {
                WXPayModelBasic FAIL = new WXPayModelBasic();
                FAIL.return_code = XmlUtil.XmlCATA("FAIL");
                FAIL.return_msg = XmlUtil.XmlCATA("验证不通过");
                return XmlUtil.SinClassToXml(FAIL);
            }
        }

        public static string GetPostData()
        {
            if (System.Web.HttpContext.Current.Request.HttpMethod.ToLower() == "post")
            {
                int length = System.Web.HttpContext.Current.Request.TotalBytes;
                byte[] binary = System.Web.HttpContext.Current.Request.BinaryRead(length);
                string requst = Encoding.UTF8.GetString(binary);
                return requst;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 新的创建 Sing包 方法
        /// <para>以后创建sing包请使用该方法</para>
        /// </summary>
        /// <param name="sodict">自动排序</param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string CreateSign(SortedDictionary<string, object> sodict, string key)
        {
            StringBuilder stringA = new StringBuilder();
            foreach (var item in sodict)
            {
                if ((string)item.Value != "")
                {
                    stringA.AppendFormat("{0}={1}&", item.Key, item.Value);
                }
            }
            string stringSignTemp = stringA.AppendFormat("key={0}", key).ToString();
            return CreateMD5(stringSignTemp).ToUpper();
        }

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string CreateMD5(string str)
        {
            return BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(str))).Replace("-", "");
        }
    }


}
