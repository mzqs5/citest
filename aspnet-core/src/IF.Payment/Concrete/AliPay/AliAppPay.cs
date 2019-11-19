using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using IF.Payment.Abstracts;
using IF.Payment.Common;
using IF.Payment.Concrete.AliPay.Model;
using IF.Payment.Model;

namespace IF.Payment.SDK.AppPay
{
    public class AliAppPay : PayBase
    {
        PayRst rst = new PayRst();
        public Dictionary<string, string> PayInfo = new Dictionary<string, string>();
        /// <summary>
        /// 第三方支付
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        protected override PayRst Pay(QueryBase query)
        {
            var Payparam = query as AliQuery;
            if (!Verification(Payparam))
            {
                return rst;
            }
            string Order_num = Payparam.order_no;
            string total_fee = String.Format("{0:F}", Payparam.Money);
            string subject = Payparam.ProductName;
            //收银台页面上，商品展示的超链接，必填

            string strJson = string.Empty;

            //不包含Encode的字符串拼接
            PayInfo.Add("app_id", Payparam.Seller_id);
            PayInfo.Add("biz_content", GetBizContent(Payparam));
            PayInfo.Add("charset", "utf-8");
            PayInfo.Add("format", "json");
            PayInfo.Add("method", "alipay.trade.app.pay");
            PayInfo.Add("notify_url", Payparam.NotifyUrl);
            PayInfo.Add("sign_type", "RSA2");
            PayInfo.Add("timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            PayInfo.Add("version", "1.0");
            string orderInfo = BuildQueryWithOutEncode(PayInfo);

            Log.log("PayLogger", "请求参数:" + orderInfo);
            // 对订单做RSA 签名
            string sign = RSAFromPkcs8.sign(orderInfo, Payparam.Key, "utf-8");
            //仅需对sign做URL编码
            sign = HttpUtility.UrlEncode(sign, Encoding.UTF8);
            string payInfo = GetOrderInfoWithEncode() + "&sign=" + sign;
            strJson = payInfo.Replace("+", "%20");//日期那里会有一个空格(2017-01-05 11:11:11)转化为+,所以这里要替换一下
            Log.log("PayLogger", "支付宝串:" + strJson);

            rst.Status = true;
            rst.Content = strJson;
            return rst;
        }


        private bool Verification(AliQuery info)
        {
            if (info == null) { rst.Status = false; rst.ErrorMsg = "支付参数不能为空"; return false; }
            if (info.Partner == null || info.Partner == "") { rst.Status = false; rst.ErrorMsg = "合作者ID不能为空"; return false; }
            if (info.Seller_id == null || info.Seller_id == "") { rst.Status = false; rst.ErrorMsg = "合作者ID不能为空"; return false; }
            if (info.Key == null || info.Key == "") { rst.Status = false; rst.ErrorMsg = "秘钥不能为空"; return false; }
            if (info.NotifyUrl == null || info.NotifyUrl == "") { rst.Status = false; rst.ErrorMsg = "商户订单号无效,无法发起本次交易,交易已取消"; return false; }
            if (info.ReturnUrl == null || info.ReturnUrl == "") { rst.Status = false; rst.ErrorMsg = "商品描述无效,无法发起本次交易,交易已取消"; return false; }
            if (info.Money == null || info.Money == "") { rst.Status = false; rst.ErrorMsg = "支付金额不能为空"; return false; }
            if (info.order_no == null || info.order_no == "") { rst.Status = false; rst.ErrorMsg = "订单号不能为空"; return false; }
            if (info.ProductName == null || info.ProductName == "") { rst.Status = false; rst.ErrorMsg = "产品名称不能为空"; return false; }
            double money = 0;
            if (!double.TryParse(info.Money, out money)) { rst.Status = false; rst.ErrorMsg = "支付金额错误！"; return false; }
            return true;
        }

        /// <summary>
        /// 包含Encode的字符串拼接
        /// </summary>
        /// <param name="price"></param>
        /// <returns></returns>
        public string GetOrderInfoWithEncode()
        {
            string strUrl = BuildQuery(PayInfo, "utf-8");
            return strUrl;
        }
        /// <summary>
        /// 获取支付内容详情
        /// </summary>
        /// <param name="total_amount"></param>
        /// <returns></returns>
        public string GetBizContent(AliQuery para)
        {
            Dictionary<string, string> biz_content_info = new Dictionary<string, string>();
            biz_content_info.Add("timeout_express", "30m");//该笔订单允许的最晚付款时间，逾期将关闭交易。
            biz_content_info.Add("seller_id", "");//收款支付宝用户ID。 如果该值为空，则默认为商户签约账号对应的支付宝用户ID
            biz_content_info.Add("product_code", "QUICK_MSECURITY_PAY");//销售产品码，商家和支付宝签约的产品码，为固定值QUICK_MSECURITY_PAY
            biz_content_info.Add("total_amount", para.Money.ToString());//订单总金额，单位为元，精确到小数点后两位，取值范围[0.01,100000000]
            biz_content_info.Add("subject", para.ProductName);//商品的标题/交易标题/订单标题/订单关键字等。
            biz_content_info.Add("body", para.ProductName);//对一笔交易的具体描述信息。如果是多种商品，请将商品描述字符串累加传给body。
            biz_content_info.Add("out_trade_no", para.order_no);//商户网站唯一订单号
            string strBizContent = JsonUtil.ToJson(biz_content_info);
            return strBizContent;
        }


        /// <summary>
        /// 组装普通文本请求参数(带Encode)。
        /// </summary>
        /// <param name="parameters">Key-Value形式请求参数字典</param>
        /// <returns>URL编码后的请求数据</returns>
        public static string BuildQuery(IDictionary<string, string> parameters, string charset)
        {
            StringBuilder postData = new StringBuilder();
            bool hasParam = false;

            IEnumerator<KeyValuePair<string, string>> dem = parameters.GetEnumerator();
            while (dem.MoveNext())
            {
                string name = dem.Current.Key;
                string value = dem.Current.Value;
                // 忽略参数名或参数值为空的参数
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
                {
                    if (hasParam)
                    {
                        postData.Append("&");
                    }

                    postData.Append(name);
                    postData.Append("=");

                    string encodedValue = HttpUtility.UrlEncode(value, Encoding.GetEncoding(charset));

                    postData.Append(encodedValue);
                    hasParam = true;
                }
            }
            return postData.ToString();
        }
        /// <summary>
        /// 组装普通文本请求参数(不带Encode)。
        /// </summary>
        /// <param name="parameters">Key-Value形式请求参数字典</param>
        /// <returns>URL编码后的请求数据</returns>
        public static string BuildQueryWithOutEncode(IDictionary<string, string> parameters)
        {
            StringBuilder postData = new StringBuilder();
            bool hasParam = false;

            IEnumerator<KeyValuePair<string, string>> dem = parameters.GetEnumerator();
            while (dem.MoveNext())
            {
                string name = dem.Current.Key;
                string value = dem.Current.Value;
                // 忽略参数名或参数值为空的参数
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
                {
                    if (hasParam)
                    {
                        postData.Append("&");
                    }

                    postData.Append(name);
                    postData.Append("=");

                    string encodedValue = value;
                    postData.Append(encodedValue);
                    hasParam = true;
                }
            }
            return postData.ToString();
        }
    }
}
