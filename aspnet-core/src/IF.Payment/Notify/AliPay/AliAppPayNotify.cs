//using Alipay.AopSdk.Core.Util;
//using System;
//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.Web;
//using IF.Payment.Common;
//using IF.Payment.Model;
//using IF.Payment.Notify;

//namespace IF.Payment.AppPay
//{
//    public class AliAppPayNotify
//    {
//        /// <summary>
//        /// 支付宝支付通知
//        /// </summary>
//        public static string Notify(Func<string, string, bool> func)
//        {
//            SortedDictionary<string, string> sPara = GetRequestPost();
//            if (sPara.Count <= 0)//判断是否有带返回参数
//                return "fail";
//            var isok = false;
//            try
//            {
//                Log.log("PayLogger", "支付宝App支付响应报文" + Core.CreateLinkString(FilterPara(sPara)));
//                bool flag = AlipaySignature.RSACheckV1(sPara, PayConfig.GetJson(PayParamType.AliPay), "utf-8", "RSA2", false);

//                if (flag)//验证成功
//                {
//                    string out_trade_no = sPara["out_trade_no"];//原支付请求的商户订单号
//                    string trade_no = sPara["trade_no"];//支付宝交易凭证号
//                    if (sPara["trade_status"] == "TRADE_FINISHED")
//                    {
//                        isok = func(out_trade_no, trade_no);
//                    }
//                    else if (sPara["trade_status"] == "TRADE_SUCCESS")
//                    {
//                        isok = func(out_trade_no, trade_no);
//                    }
//                    else
//                    {
//                        Log.log("PayLogger", "订单状态错误！订单号:" + out_trade_no + "，参数信息:" + JsonUtil.ToJson(HttpContext.Current.Request.QueryString));
//                    }
//                }
//                else//验证失败
//                {
//                    Log.log("PayLogger", "支付宝App支付通知验证失败");
//                }
//            }
//            catch (Exception ex)
//            {
//                Log.log("PayLogger", "支付宝App支付异常:" + ex.Message);
//            }
//            return isok ? "success" : "fail"; //请不要修改或删除
//        }
//        /// <summary>
//        /// 获取支付宝POST过来通知消息，并以“参数名=参数值”的形式组成数组
//        /// </summary>
//        /// <returns>request回来的信息组成的数组</returns>
//        public static SortedDictionary<string, string> GetRequestPost()
//        {
//            int i = 0;
//            SortedDictionary<string, string> sArray = new SortedDictionary<string, string>();
//            NameValueCollection coll;
//            //Load Form variables into NameValueCollection variable.
//            coll = HttpContext.Current.Request.Form;

//            // Get names of all forms into a string array.
//            String[] requestItem = coll.AllKeys;
//            for (i = 0; i < requestItem.Length; i++)
//            {
//                sArray.Add(requestItem[i], HttpContext.Current.Request.Form[requestItem[i]]);
//            }
//            Log.log("PayLogger", "支付宝App通知参数:" + JsonUtil.ToJson(sArray));
//            return sArray;
//        }

//        public static Dictionary<string, string> FilterPara(SortedDictionary<string, string> dicArrayPre)
//        {
//            Dictionary<string, string> dicArray = new Dictionary<string, string>();
//            foreach (KeyValuePair<string, string> temp in dicArrayPre)
//            {
//                dicArray.Add(temp.Key, temp.Value);
//            }

//            return dicArray;
//        }
//    }
//}
