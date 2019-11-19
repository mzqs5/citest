using System;
using System.Collections;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using IF.Payment.Abstracts;
using IF.Payment.Common;
using IF.Payment.Concrete.WeChatPay.Model;
using IF.Payment.Model;

namespace IF.Payment.Concrete.WeChatPay
{
    public class WXAppPay : PayBase
    {
        public PayRst rst = new PayRst();

        /// <summary>
        /// 第三方支付
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        protected override PayRst Pay(QueryBase query)
        {
            var Payparam = query as WXQuery;
            if (!Verification(Payparam))
            {
                return rst;
            }

            string prepayId = "";

            string timeStamp = WXAPI.Pay_TenpayUtil.getTimestamp(); //时间戳   
            string nonceStr = WXAPI.Pay_TenpayUtil.getNoncestr();    //

            //****************************************************************获取预支付订单编号***********************
            //设置package订单参数
            Hashtable packageParameter = new Hashtable();
            packageParameter.Add("appid", Payparam.AppId);//开放账号ID  
            packageParameter.Add("mch_id", Payparam.Mch_id); //商户号
            packageParameter.Add("nonce_str", nonceStr); //随机字符串
            packageParameter.Add("body", Payparam.ProductName); //商品描述    
            packageParameter.Add("out_trade_no", Payparam.order_no); //商家订单号 
            packageParameter.Add("total_fee", (Math.Round(Convert.ToDecimal(Payparam.Money) * 100, 0)).ToString()); //商品金额,以分为单位    
            packageParameter.Add("spbill_create_ip", HttpContext.Current.Request.UserHostAddress); //订单生成的机器IP，指用户浏览器端IP  
            packageParameter.Add("notify_url", Payparam.NotifyUrl); //接收财付通通知的URL  
            packageParameter.Add("trade_type", "APP");//交易类型  
            packageParameter.Add("fee_type", "CNY"); //币种，1人民币   66  

            Log.log("PayLogger", "微信App支付package订单参数：" + JsonUtil.ToJson(packageParameter));

            var sign = CreateMd5Sign("key", Payparam.Appkey, packageParameter, HttpContext.Current.Request.ContentEncoding.BodyName);
            //拼接上签名
            packageParameter.Add("sign", sign);
            //生成加密包的XML格式字符串
            string data = parseXML(packageParameter);
            //调用统一下单接口，获取预支付订单号码
            string prepayXml = Utils.HTTP_POST("https://api.mch.weixin.qq.com/pay/unifiedorder", data);
            Log.log("PayLogger", "微信App支付预支付返回结果：" + prepayXml);
            //获取预支付ID

            var xdoc = new XmlDocument();
            xdoc.LoadXml(prepayXml);
            XmlNode xn = xdoc.SelectSingleNode("xml");
            XmlNodeList xnl = xn.ChildNodes;
            if (xnl.Count > 7)
            {
                prepayId = xnl[7].InnerText;
            }

            //**************************************************封装调起微信客户端支付界面字符串********************
            //设置待加密支付参数并加密
            Hashtable paySignReqHandler = new Hashtable();
            paySignReqHandler.Add("appid", Payparam.AppId);
            paySignReqHandler.Add("partnerid", Payparam.Mch_id);
            paySignReqHandler.Add("prepayid", prepayId);
            paySignReqHandler.Add("package", "Sign=WXPay");
            paySignReqHandler.Add("noncestr", nonceStr);
            paySignReqHandler.Add("timestamp", timeStamp);
            var paySign = CreateMd5Sign("key", Payparam.Appkey, paySignReqHandler, HttpContext.Current.Request.ContentEncoding.BodyName);

            //设置支付包参数
            Wx_Pay_Model wxpaymodel = new Wx_Pay_Model();
            wxpaymodel.retcode = 0;//5+固定调起参数
            wxpaymodel.retmsg = "ok";//5+固定调起参数
            wxpaymodel.appid = Payparam.AppId;//AppId,微信开放平台新建应用时产生
            wxpaymodel.partnerid = Payparam.Mch_id;//商户编号，微信开放平台申请微信支付时产生
            wxpaymodel.prepayid = prepayId;//由上面获取预支付流程获取
            wxpaymodel.package = "Sign=WXpay";//APP支付固定设置参数
            wxpaymodel.noncestr = nonceStr;//随机字符串，
            wxpaymodel.timestamp = timeStamp;//时间戳
            wxpaymodel.sign = paySign;//上面关键参数加密获得
            Log.log("PayLogger", "微信App支付包参数：" + JsonUtil.ToJson(wxpaymodel));
            rst.Status = true;
            rst.Content = JsonUtil.ToJson(wxpaymodel);
            rst.ErrorMsg = "";
            return rst;
        }


        /// <summary>
        /// 验证支付参数
        /// </summary>
        /// <returns></returns>
        public bool Verification(WXQuery info)
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

        /// <summary>
        /// 将类对象拼接成调起支付字符串
        /// </summary>
        /// <param name="_model"></param>
        /// <returns></returns>
        private string ReSetPayString(Wx_Pay_Model _model)
        {
            StringBuilder strpay = new StringBuilder();
            PropertyInfo[] props = _model.GetType().GetProperties();
            strpay.Append("{");
            foreach (PropertyInfo property in props)
            {
                strpay.Append(property.Name + ":\"" + property.GetValue(_model, null).ToString() + "\",");
            }
            strpay.Remove(strpay.Length - 1, 1);
            strpay.Append("}");
            return strpay.ToString();
        }

        /// <summary>
        /// 输出XML
        /// </summary>
        /// <returns></returns>
        public string parseXML(Hashtable _parameters)
        {
            var sb = new StringBuilder();
            sb.Append("<xml>");
            var akeys = new ArrayList(_parameters.Keys);
            foreach (string k in akeys)
            {
                var v = (string)_parameters[k];
                if (Regex.IsMatch(v, @"^[0-9.]$"))
                {
                    sb.Append("<" + k + ">" + v + "</" + k + ">");
                }
                else
                {
                    sb.Append("<" + k + "><![CDATA[" + v + "]]></" + k + ">");
                }
            }
            sb.Append("</xml>");
            return sb.ToString();
        }

        /// <summary>
        /// 创建package签名
        /// </summary>
        /// <param name="key">密钥键</param>
        /// <param name="value">财付通商户密钥（自定义32位密钥）</param>
        /// <returns></returns>
        public virtual string CreateMd5Sign(string key, string value, Hashtable parameters, string _ContentEncoding)
        {
            var sb = new StringBuilder();
            //数组化键值对，并排序
            var akeys = new ArrayList(parameters.Keys);
            akeys.Sort();
            //循环拼接包参数
            foreach (string k in akeys)
            {
                var v = (string)parameters[k];
                if (null != v && "".CompareTo(v) != 0
                    && "sign".CompareTo(k) != 0 && "key".CompareTo(k) != 0)
                {
                    sb.Append(k + "=" + v + "&");
                }
            }
            //最后拼接商户自定义密钥
            sb.Append(key + "=" + value);
            //加密
            string sign = MD5Util.GetMD5(sb.ToString(), _ContentEncoding).ToUpper();
            //返回密文
            return sign;
        }

    }
}
