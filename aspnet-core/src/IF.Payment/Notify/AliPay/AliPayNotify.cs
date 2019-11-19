using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using IF.Payment.Common;
using IF.Payment.Model;

namespace IF.Payment.Notify
{
    /// <summary>
    /// 类名：Notify
    /// 功能：支付宝通知处理类
    /// 详细：处理支付宝各接口通知返回
    /// 版本：1.0
    /// 修改日期：2017-12-23
    /// dawu
    /// //////////////////////注意/////////////////////////////
    /// 调试通知返回时，可查看或改写log日志的写入TXT里的数据，来检查通知返回是否正常 
    /// </summary>
    public class AliPayNotify
    {
        /// <summary>
        /// 支付宝支付通知
        /// </summary>
        public static string Notify(Func<string, string, bool> func)
        {
            SortedDictionary<string, string> sPara = GetRequestPost();
            if (sPara.Count <= 0)//判断是否有带返回参数
                return "fail";
            AliPayNotifyHelper aliNotify = new AliPayNotifyHelper(PayConfig.GetMerchantNo(PayParamType.AliPay), PayConfig.GetPaySecret(PayParamType.AliPay), "utf-8", "MD5");
            bool verifyResult = aliNotify.Verify(sPara, sPara["notify_id"], sPara["sign"]);

            if (verifyResult)//验证成功
            {
                string out_trade_no = sPara["out_trade_no"];//原支付请求的商户订单号
                string trade_no = sPara["trade_no"];//支付宝交易凭证号
                var isok = false;
                if (sPara["trade_status"] == "TRADE_FINISHED")
                {
                    isok = func(out_trade_no, trade_no);
                }
                else if (sPara["trade_status"] == "TRADE_SUCCESS")
                {
                    isok = func(out_trade_no, trade_no);
                }
                else
                {
                    Log.log("PayLogger", "订单状态错误！订单号:" + out_trade_no + "，参数信息:" + JsonUtil.ToJson(HttpContext.Current.Request.QueryString));
                }
                return isok ? "success" : "fail"; //请不要修改或删除
            }
            else//验证失败
                return "fail";
        }
        /// <summary>
        /// 获取支付宝POST过来通知消息，并以“参数名=参数值”的形式组成数组
        /// </summary>
        /// <returns>request回来的信息组成的数组</returns>
        public static SortedDictionary<string, string> GetRequestPost()
        {
            int i = 0;
            SortedDictionary<string, string> sArray = new SortedDictionary<string, string>();
            NameValueCollection coll;
            //Load Form variables into NameValueCollection variable.
            coll = HttpContext.Current.Request.Form;

            // Get names of all forms into a string array.
            String[] requestItem = coll.AllKeys;
            for (i = 0; i < requestItem.Length; i++)
            {
                sArray.Add(requestItem[i], HttpContext.Current.Request.Form[requestItem[i]]);
            }
            Log.log("PayLogger", "支付宝通知参数:" + JsonUtil.ToJson(sArray));
            return sArray;
        }

    }
}