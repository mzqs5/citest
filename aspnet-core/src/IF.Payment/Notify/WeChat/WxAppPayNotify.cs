
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Text;
//using System.Xml;
//using IF.Payment.Common;
//using IF.Payment.Model;

//namespace IF.Payment.AppPay
//{
//    public class WxAppPayNotify
//    {
//        /// <summary>
//        /// 微信支付通知
//        /// </summary>
//        public static string Notify(Func<string, string, bool> func)
//        {
//            string result = "failed";
//            try
//            {
//                //得到微信推送来的xml数据
//                SortedDictionary<string, string> requestXML = GetInfoFromXml();
//                if (requestXML != null && requestXML.Count > 0)
//                {

//                    #region 验签
//                    //微信返回的签名字符串
//                    string sign = requestXML["sign"];
//                    requestXML.Remove("sign");
//                    //待签名字符串
//                    string signStr = AlipaySignature.GetSignContent(requestXML) + "&key=" + PayConfig.GetPaySecret(PayParamType.WeChatPay);//借用阿里的方法
//                    string newsign = MD5Util.GetMD5(signStr, "UTF-8").ToUpper();//MD5加密，转大写
//                    bool ValidateSign = sign == newsign;//验证签名是否一致
//                    #endregion

//                    #region 处理订单
//                    if (ValidateSign)
//                    {
//                        var isok = func(requestXML["out_trade_no"], requestXML["transaction_id"]);
//                        if (isok)
//                            result = "success";
//                    }
//                    else
//                    {
//                        result = "CheckSignError";
//                        Log.log("PayLogger", "WeiXinPayController.Notify【验签失败】");
//                    }
//                    #endregion
//                }
//            }
//            catch (Exception ex)
//            {
//                Log.log("PayLogger", "WeiXinPayController.Notify【程序异常】" + ex.Message);
//                result = "exception";
//            }

//            string xmlstr = @"<xml><return_code><![CDATA[SUCCESS]]></return_code><return_msg><![CDATA[OK]]></return_msg></xml>";
//            if (result != "success")
//            {
//                xmlstr = @"<xml><return_code><![CDATA[FAIL]]></return_code><return_msg><![CDATA[FAIL]]></return_msg></xml>";
//            }
//            return xmlstr;
//        }
//        /// <summary>
//        /// 把XML数据转换为SortedDictionary<string, string>集合
//        /// </summary>
//        /// <param name="strxml"></param>
//        /// <returns></returns>
//        public static SortedDictionary<string, string> GetInfoFromXml()
//        {
//            Stream s = System.Web.HttpContext.Current.Request.InputStream;
//            byte[] b = new byte[s.Length];
//            s.Read(b, 0, (int)s.Length);
//            string postStr = Encoding.UTF8.GetString(b);
//            SortedDictionary<string, string> sParams = new SortedDictionary<string, string>();
//            try
//            {
//                XmlDocument doc = new XmlDocument();
//                doc.LoadXml(postStr);
//                XmlElement root = doc.DocumentElement;
//                int len = root.ChildNodes.Count;
//                for (int i = 0; i < len; i++)
//                {
//                    string name = root.ChildNodes[i].Name;
//                    if (!sParams.ContainsKey(name))
//                    {
//                        sParams.Add(name.Trim(), root.ChildNodes[i].InnerText.Trim());
//                    }
//                }
//            }
//            catch { }
//            return sParams;
//        }
//    }
//}
