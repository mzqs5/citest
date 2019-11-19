//using System;
//using System.Xml;
//using IF.Payment.Abstracts;
//using IF.Payment.Common;
//using IF.Payment.Model;
//using IF.Payment.Concrete.WeChatPay.Model;
//using WXAPI;

//namespace IF.Payment.Concrete.WeChatPay
//{
//    class WXH5Pay : PayBase
//    {
//        public PayRst rst = new PayRst();


//        /// <summary>
//        /// 第三方支付
//        /// </summary>
//        /// <param name="query"></param>
//        /// <returns></returns>
//        protected override PayRst Pay(QueryBase query)
//        {
//            var Payparam = query as WXH5Query;
//            if (!Verification(Payparam))
//            {
//                return rst;
//            }

//            string total_fee = Convert.ToInt32((Convert.ToDouble(Payparam.Money) * 100)).ToString();
//            //创建支付应答对象
//            var packageReqHandler = new Pay_RequestHandler(System.Web.HttpContext.Current);
//            //初始化
//            packageReqHandler.init();

//            string timeStamp = WXAPI.Pay_TenpayUtil.getTimestamp(); //时间戳   
//            string nonceStr = WXAPI.Pay_TenpayUtil.getNoncestr();    //

//            //设置package订单参数 
//            packageReqHandler.setParameter("body", Payparam.ProductName); //商品信息 127字符
//            packageReqHandler.setParameter("appid", Payparam.AppId);
//            packageReqHandler.setParameter("mch_id", Payparam.Mch_id);
//            packageReqHandler.setParameter("nonce_str", nonceStr.ToLower());
//            packageReqHandler.setParameter("notify_url", Payparam.NotifyUrl);
//            //packageReqHandler.setParameter("sub_mch_id", Payparam.Sub_mch_id);
//            packageReqHandler.setParameter("attach", "支付：" + total_fee.ToString());
//            packageReqHandler.setParameter("out_trade_no", Payparam.order_no); //商家订单号
//            packageReqHandler.setParameter("spbill_create_ip", Payparam.Spbill_create_ip); //用户的公网ip，不是商户服务器IP
//            packageReqHandler.setParameter("total_fee", total_fee.ToString()); //商品金额,以分为单位(money * 100).ToString()
//            packageReqHandler.setParameter("trade_type", "MWEB");
//            //WAP网站应用{"h5_info": //h5支付固定传"h5_info" {"type": "",  //场景类型 "wap_url": "",//WAP网站URL地址"wap_name": ""  //WAP 网站名}}

//            packageReqHandler.setParameter("scene_info", "{\"h5_info\": {\"type\":\"Wap\",\"wap_url\": \"" + Payparam.Wap_Url + "\",\"wap_name\": \"" + Payparam.Wap_name + "\"}}");

//            //获取package包
//            string sign = packageReqHandler.CreateMd5Sign("key", Payparam.Appkey);
//            packageReqHandler.setParameter("sign", sign);

//            string XMLdata = packageReqHandler.parseXML();
//            Log.log("PayLogger", "key:请求参数 value:" + XMLdata);
//            string prepayXml = WXAPI.Pay_HttpUtil.Send(XMLdata, "https://api.mch.weixin.qq.com/pay/unifiedorder"); //

//            Log.log("PayLogger", "key:请求结果 value:" + prepayXml);
//            //获取预支付ID
//            if (prepayXml != "")
//            {
//                XmlDocument doc = new XmlDocument();
//                doc.LoadXml(prepayXml);
//                XmlElement rootElement = doc.DocumentElement;
//                string return_code = rootElement.SelectSingleNode("return_code").InnerText;
//                if (rootElement.SelectSingleNode("err_code_des") != null)
//                {
//                    rst.Status = false;
//                    rst.Content = rootElement.SelectSingleNode("err_code_des").InnerText;
//                    return rst;
//                }
//                if (return_code.ToUpper() == "SUCCESS") //成功
//                {
//                    string result_code = rootElement.SelectSingleNode("result_code").InnerText;
//                    if (result_code.ToUpper() == "SUCCESS") //成功
//                    {
//                        rst.Status = true;
//                        rst.Content = rootElement.SelectSingleNode("mweb_url").InnerText;
//                    }
//                }
//            }

//            return rst;

//        }

//        /// <summary>
//        /// 验证支付参数
//        /// </summary>
//        /// <returns></returns>
//        public bool Verification(WXH5Query info)
//        {
//            if (info == null) { rst.Status = false; rst.ErrorMsg = "支付参数不能为空"; return false; }
//            if (info.AppId == null || info.AppId == "") { rst.Status = false; rst.ErrorMsg = "mchid值为空,无法发起本次交易,交易已取消"; return false; }
//            if (info.Mch_id == null || info.Mch_id == "") { rst.Status = false; rst.ErrorMsg = "mchid值无效,无法发起本次交易,交易已取消"; return false; }
//            if (info.Appkey == null || info.Appkey == "") { rst.Status = false; rst.ErrorMsg = "appkey值无效,无法发起本次交易,交易已取消"; return false; }
//            if (info.order_no == null || info.order_no == "") { rst.Status = false; rst.ErrorMsg = "商户订单号无效,无法发起本次交易,交易已取消"; return false; }
//            if (info.ProductName == null || info.ProductName == "") { rst.Status = false; rst.ErrorMsg = "商品描述无效,无法发起本次交易,交易已取消"; return false; }
//            if (info.Wap_name == null || info.Wap_name == "") { rst.Status = false; rst.ErrorMsg = "网站名称不能为空"; return false; }
//            if (info.Wap_Url == null || info.Wap_Url == "") { rst.Status = false; rst.ErrorMsg = "网站地址不能为空"; return false; }
//            //if (info.Sub_mch_id == null || info.Sub_mch_id == "") { rst.Status = false; rst.ErrorMsg = "字商户号不能为空"; return false; }
//            double money = 0;
//            if (!double.TryParse(info.Money, out money)) { rst.Status = false; rst.ErrorMsg = "支付金额错误！"; return false; }
//            return true;
//        }
//    }
//}
