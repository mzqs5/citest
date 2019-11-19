using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

/// <summary>
/// PayHelp 的摘要说明
/// </summary>
public class PayHelp
{
	public PayHelp()
	{
		//
		// TODO: 在此处添加构造函数逻辑
		//
	}
    /// <summary>
    /// 根据ASCII排序 获取大写的DM5 签名
    /// </summary>
    /// <param name="sParaTemp">参数</param>
    /// <param name="key">秘钥</param>
    /// <returns></returns>
    public static string CreateSign(SortedDictionary<string, string> sParaTemp, string key)
    {
        StringBuilder sb = new StringBuilder();

        ArrayList akeys = new ArrayList(sParaTemp.Keys);
        akeys.Sort();

        foreach (string k in akeys)
        {
            string v = (string)sParaTemp[k];
            if (null != v && "".CompareTo(v) != 0
                && "sign".CompareTo(k) != 0 && "key".CompareTo(k) != 0)
            {
                sb.Append(k + "=" + v + "&");
            }
        }

        sb.Append("paySecret=" + key);

        string sign = MD5Util.GetMD5(sb.ToString(), "utf-8");

        return sign;
    }


    public static string GetWXPayHtml(string payjs, string url, string cancelurl)
    {
        StringBuilder sbHtml = new StringBuilder();
        //sbHtml.Append("<html xmlns=\"http://www.w3.org/1999/xhtml\">");
        //sbHtml.Append("<head runat=\"server\">");
        //sbHtml.Append("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"/>");
        //sbHtml.Append(" <title></title>");
        //sbHtml.Append(" </head>");
        //sbHtml.Append(" <body>");
        //sbHtml.Append(" <form id=\"form1\" runat=\"server\">");
        //sbHtml.Append(" </form>");
        //sbHtml.Append("</body><script>");

        sbHtml.Append(" function jsApiCall() {WeixinJSBridge.invoke( 'getBrandWCPayRequest'," + payjs + ", function (res) {");
        sbHtml.Append(" if (res.err_msg == 'get_brand_wcpay_request:ok') { window.location.href='" + url + "';}");
        sbHtml.Append(" else{if(confirm(\"支付已取消！\")){window.location.href='" + cancelurl + "';}}});} if (typeof WeixinJSBridge == 'undefined') {");
        sbHtml.Append(" if (document.addEventListener) { document.addEventListener('WeixinJSBridgeReady', jsApiCall, false);}");
        sbHtml.Append("  else if (document.attachEvent) { document.attachEvent('WeixinJSBridgeReady', jsApiCall);document.attachEvent('onWeixinJSBridgeReady', jsApiCall);} }");
        sbHtml.Append("else {jsApiCall();}");
        //sbHtml.Append("</script></html>");

        return sbHtml.ToString();
    }

    /// <summary>
    /// 拼接支付参数
    /// </summary>
    /// <param name="sParaTemp">支付参数</param>
    /// <param name="sign">签名</param>
    /// <returns></returns>
    public static string GetPayData(SortedDictionary<string, string> sParaTemp, string sign)
    {
        StringBuilder sb = new StringBuilder();
        ArrayList akeys = new ArrayList(sParaTemp.Keys);
        foreach (string k in akeys)
        {
            string v = (string)sParaTemp[k];
            sb.Append(k + "=" + v + "&");
        }
        sb.Append("sign=" + sign);
        return sb.ToString();

    }
    /// <summary>
    /// 获取用户的IP地址
    /// </summary>
    /// <returns></returns>
    public static string GetUserIP()
    {
        string IP = HttpContext.Current.Request.UserHostAddress;

        if (string.IsNullOrEmpty(IP))
        {
            IP = (HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null && HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != String.Empty) ? HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] : HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]; 

        }

        if (string.IsNullOrEmpty(IP))
        {
            IP = HttpContext.Current.Request.UserHostAddress;
        }
        //最后判断获取是否成功，并检查IP地址的格式（检查其格式非常重要）
        if (!string.IsNullOrEmpty(IP) && IsIP(IP))
        {
        }
        else
        {
            IP = "127.0.0.1";
        }
        return IP;
    }

    /// <summary>
    /// 检查IP地址格式
    /// </summary>
    /// <param name="ip"></param>
    /// <returns></returns>  
    public static bool IsIP(string ip)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
    }

    /// <summary>
    /// http请求
    /// </summary>
    /// <param name="url"></param>
    /// <param name="pdata"></param>
    /// <param name="contentType"></param>
    /// <param name="encode"></param>
    /// <returns></returns>
    public static string Post(string url, string pdata, string contentType=null, string encode="utf-8")
    {
        System.GC.Collect(); //强制回收非正常的连接
        if (contentType == null) { contentType =  "application/x-www-form-urlencoded"; }
        Encoding encoding = Encoding.GetEncoding(encode.ToLower());
        byte[] data = encoding.GetBytes(pdata);
        try
        {
            var request = WebRequest.Create(url) as HttpWebRequest;
            request.AllowAutoRedirect = true;
            request.Method = "POST";
            request.ContentType = contentType;
            request.ContentLength = data.Length;
            var outstream = request.GetRequestStream();
            outstream.Write(data, 0, data.Length);
            outstream.Close();

            var response = request.GetResponse() as HttpWebResponse;
            var instream = response.GetResponseStream();
            var sr = new StreamReader(instream, encoding);
            var rs = sr.ReadToEnd();
            sr.Close();
            response.Close();
            return rs;
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }





    /// <summary>
    /// 表单提交
    /// </summary>
    /// <param name="sParaTemp">参数</param>
    /// <param name="action">地址</param>
    /// <param name="strMethod">提交方式  post、get</param>
    /// <param name="_input_charset">编码格式（UTF-8）</param>
    /// <param name="strButtonValue">按钮名称</param>
    /// <returns></returns>
    public static string BuildRequest(SortedDictionary<string, string> sParaTemp, string action, string strMethod, string _input_charset, string strButtonValue)
    {
        StringBuilder RowString = new StringBuilder();
        RowString.Append("<!DOCTYPE html>\r\n");
        RowString.Append("<html xmlns=\"http://www.w3.org/1999/xhtml\">\r\n");
        RowString.Append("<head>\r\n");
        RowString.Append("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"/>\r\n");
        RowString.Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0, minimum-scale=1.0, maximum-scale=1.0, user-scalable=no\"/>\r\n");
        RowString.Append("<title>支付宝</title>\r\n");
        RowString.Append("</head>\r\n");
        RowString.Append("<body>\r\n");

        StringBuilder sbHtml = new StringBuilder();

        sbHtml.Append("<form id='alipaysubmit' name='alipaysubmit' action='" + action + "_input_charset=" + _input_charset + "' method='" + strMethod.ToLower().Trim() + "'>");

        foreach (KeyValuePair<string, string> temp in sParaTemp)
        {
            sbHtml.Append("<input type='hidden' name='" + temp.Key + "' value='" + temp.Value + "'/>");
        }

        //submit按钮控件请不要含有name属性
        sbHtml.Append("<input id=\"PayBt\" type='submit' value='" + strButtonValue + "' style='display:none;'></form>");
        sbHtml.Append("<script type=\"text/javascript\">document.getElementById(\"PayBt\").click();</script>");
        //sbHtml.Append("<script type=\"text/javascript\"> function GoPay(){document.getElementById(\"PayBt\").click();} window.setInterval(\"GoPay()\",100);</script>");
        //RowString.Append("<div style=\"width:100%;text-align:center;padding-top:80px;\">正在跳转到支付宝 ...</div>");
        RowString.Append(sbHtml.ToString());
        RowString.Append("</body></html>\r\n");

        return RowString.ToString();
    }




}