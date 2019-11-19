
using System;
using System.Collections.Generic;
using System.Web;
using IF.Payment.Abstracts;
using IF.Payment.Common;
using IF.Payment.Concrete.AliPay.Model;
using IF.Payment.Model;
using IF.Payment.Util.AliPay;

namespace IF.Payment.Concrete.AliPay
{
    public class AliPay : PayBase
    {
        PayRst rst = new PayRst();
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
            string show_url = (HttpContext.Current.Request.UrlReferrer == null) ? "" : HttpContext.Current.Request.UrlReferrer.ToString();

            string url = "https://mapi.alipay.com/gateway.do?";
            //把请求参数打包成数组
            SortedDictionary<string, string> sParaTemp = new SortedDictionary<string, string>();
            sParaTemp.Add("partner", Payparam.Partner);
            sParaTemp.Add("seller_id", Payparam.Seller_id);
            sParaTemp.Add("_input_charset", "utf-8");
            sParaTemp.Add("service", "alipay.wap.create.direct.pay.by.user");
            sParaTemp.Add("payment_type", "1");
            sParaTemp.Add("notify_url", Payparam.NotifyUrl);
            sParaTemp.Add("return_url", Payparam.ReturnUrl);
            sParaTemp.Add("out_trade_no", Order_num);
            sParaTemp.Add("subject", subject);
            sParaTemp.Add("total_fee", total_fee);
            sParaTemp.Add("show_url", show_url);
            //sParaTemp.Add("app_pay","Y");//启用此参数可唤起钱包APP支付。
            sParaTemp.Add("body", "");
            //其他业务参数根据在线开发文档，添加参数.文档地址:https://doc.open.alipay.com/doc2/detail.htm?spm=a219a.7629140.0.0.2Z6TSk&treeId=60&articleId=103693&docType=1
            //如sParaTemp.Add("参数名","参数值");
            foreach (KeyValuePair<string, string> temp in sParaTemp)
            {
                Log.log("AliPayLogger", "key:" + temp.Key + " value:" + temp.Value);
            }
            //建立请求
            string sHtmlText = Submit.BuildRequest(sParaTemp, "get", "确认", Payparam.Key, url);
            rst.Status = true;
            rst.Content = sHtmlText;
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
    }
}
