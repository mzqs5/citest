using System;
using System.Xml;
using IF.Payment.Abstracts;
using IF.Payment.Common;
using IF.Payment.Model;
using IF.Payment.Concrete.WeChatPay.Model;

namespace IF.Payment.Concrete.WeChatPay
{
    public class WXSMPay : PayBase
    {
        public PayRst rst = new PayRst();


        /// <summary>
        /// 第三方支付
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        protected override PayRst Pay(QueryBase query)
        {
            var Payparam = query as WXSMQuery;
            if (!Verification(Payparam))
            {
                return rst;
            }

            string total_fee = Convert.ToInt32((Convert.ToDouble(Payparam.Money) * 100)).ToString();


            //创建支付应答对象
            var packageReqHandler = new Pay_RequestHandler(query.httpContext);
            //初始化
            packageReqHandler.init();

            string timeStamp = WXAPI.Pay_TenpayUtil.getTimestamp(); //时间戳   
            string nonceStr = WXAPI.Pay_TenpayUtil.getNoncestr();    //

            //设置package订单参数 
            packageReqHandler.setParameter("body", Payparam.ProductName); //商品信息 127字符
            packageReqHandler.setParameter("appid", Payparam.AppId);
            packageReqHandler.setParameter("mch_id", Payparam.Mch_id);
            packageReqHandler.setParameter("nonce_str", nonceStr.ToLower());
            packageReqHandler.setParameter("notify_url", Payparam.NotifyUrl);
            packageReqHandler.setParameter("attach", "支付：" + total_fee.ToString());
            packageReqHandler.setParameter("out_trade_no", Payparam.order_no); //商家订单号
            packageReqHandler.setParameter("spbill_create_ip", PayHelp.GetUserIP()); //用户的公网ip，不是商户服务器IP
            packageReqHandler.setParameter("total_fee", total_fee.ToString()); //商品金额,以分为单位(money * 100).ToString()
            packageReqHandler.setParameter("trade_type", "NATIVE");
            packageReqHandler.setParameter("product_id", Payparam.Productid);//商品ID

            //获取package包
            string sign = packageReqHandler.CreateMd5Sign("key", Payparam.Appkey);
            packageReqHandler.setParameter("sign", sign);

            string XMLdata = packageReqHandler.parseXML();
            Log.log("PayLogger", "key:XMLdata value:" + XMLdata);
            string url = "https://api.mch.weixin.qq.com/pay/unifiedorder";     //请求URL

            string msgxml = WXAPI.WRequest.HTML_POST_UTF8(url, XMLdata);    //post请求

            Log.log("PayLogger", "key:微信请求结果 value:" + msgxml);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(msgxml);
            XmlElement rootElement = doc.DocumentElement;
            string return_code = rootElement.SelectSingleNode("return_code").InnerText;
            string return_msg = rootElement.SelectSingleNode("return_msg").InnerText;
            string result_code = "";
            string code_url = "";

            rst.Status = false;
            rst.ErrorMsg = "微信扫码支付请求失败!";


            if (return_code == "SUCCESS")
            {

                if (return_msg == "OK")
                {
                    result_code = rootElement.SelectSingleNode("result_code").InnerText;
                    if (result_code == "SUCCESS")
                    {
                        code_url = rootElement.SelectSingleNode("code_url").InnerText;
                        rst.Status = true;
                        rst.Content = code_url;
                    }
                    else
                    {
                        //请求失败
                    }
                }
                else
                {
                    //请求失败
                }
            }
            else
            {
                //请求失败
            }


            return rst;

        }

        /// <summary>
        /// 验证支付参数
        /// </summary>
        /// <returns></returns>
        public bool Verification(WXSMQuery info)
        {
            if (info == null) { rst.Status = false; rst.ErrorMsg = "支付参数不能为空"; return false; }
            if (info.AppId == string.Empty || info.AppId == "") { rst.Status = false; rst.ErrorMsg = "mchid值为空,无法发起本次交易,交易已取消"; return false; }
            if (info.Mch_id == string.Empty || info.Mch_id == "") { rst.Status = false; rst.ErrorMsg = "mchid值无效,无法发起本次交易,交易已取消"; return false; }
            if (info.Appkey == string.Empty || info.Appkey == "") { rst.Status = false; rst.ErrorMsg = "appkey值无效,无法发起本次交易,交易已取消"; return false; }
            if (info.order_no == string.Empty || info.order_no == "") { rst.Status = false; rst.ErrorMsg = "商户订单号无效,无法发起本次交易,交易已取消"; return false; }
            if (info.ProductName == string.Empty || info.ProductName == "") { rst.Status = false; rst.ErrorMsg = "商品描述无效,无法发起本次交易,交易已取消"; return false; }
            double money = 0;
            if (!double.TryParse(info.Money, out money)) { rst.Status = false; rst.ErrorMsg = "支付金额错误！"; return false; }
            return true;
        }


    }
}
