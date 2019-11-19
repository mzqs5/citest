using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace IF.Payment.Common
{
    /// <summary>
    /// 常用方法
    /// </summary>
    public sealed partial class Utils
    {

        #region URL处理
        /// <summary>
        /// URL字符编码
        /// </summary>
        public static string UrlEncode(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return "";
            }
            str = str.Replace("'", "");
            
            return HttpUtility.UrlEncode(str);
        }

        /// <summary>
        /// URL字符解码
        /// </summary>
        public static string UrlDecode(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return "";
            }
            return HttpUtility.UrlDecode(str);
        }
        #endregion

        #region URL请求数据
        /// <summary>
        /// HTTP POST方式请求数据
        /// </summary>
        /// <param name="url">URL.</param>
        /// <param name="param">POST的数据</param>
        /// <returns></returns>
        public static string HttpPost(string url, string param)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "*/*";
            request.Timeout = 15000;
            request.AllowAutoRedirect = false;

            StreamWriter requestStream = null;
            WebResponse response = null;
            string responseStr = null;

            try
            {
                requestStream = new StreamWriter(request.GetRequestStream());
                requestStream.Write(param);
                requestStream.Close();

                response = request.GetResponse();
                if (response != null)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                    responseStr = reader.ReadToEnd();
                    reader.Close();
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                request = null;
                requestStream = null;
                response = null;
            }

            return responseStr;
        }

        /// <summary>
        /// HTTP GET方式请求数据.
        /// </summary>
        /// <param name="url">URL.</param>
        /// <returns></returns>
        public static string HttpGet(string url)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "GET";
            //request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "*/*";
            request.Timeout = 15000;
            request.AllowAutoRedirect = false;

            WebResponse response = null;
            string responseStr = null;

            try
            {
                response = request.GetResponse();

                if (response != null)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                    responseStr = reader.ReadToEnd();
                    reader.Close();
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                request = null;
                response = null;
            }

            return responseStr;
        }

        #endregion

        #region WebClient GET

        /// <summary>
        /// 异步请求 Get方式
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string HttpGetAsync(string url)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "GET";
            request.Accept = "*/*";
            request.Timeout = 15000;
            request.AllowAutoRedirect = false;

            WebResponse response = null;
            Task<string> responseStr = null;

            try
            {
                response = request.GetResponseAsync().Result;

                if (response != null)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                    responseStr = reader.ReadToEndAsync();
                    reader.Close();
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                request = null;
                response = null;
            }

            return responseStr.Result;
        }
        /// <summary>
        /// 异步请求 Post方式
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string HttpPostAsync(string url, string param)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "*/*";
            request.Timeout = 15000;
            request.AllowAutoRedirect = false;

            StreamWriter requestStream = null;
            WebResponse response = null;
            Task<string> responseStr = null;

            try
            {
                requestStream = new StreamWriter(request.GetRequestStreamAsync().Result);
                requestStream.Write(param);
                requestStream.Close();

                response = request.GetResponseAsync().Result;
                if (response != null)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                    responseStr = reader.ReadToEndAsync();
                    reader.Close();
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                request = null;
                requestStream = null;
                response = null;
            }

            return responseStr.Result;
        }

        public static string HTTP_GET(string url)
        {
            return HTTP_GET(url, Encoding.UTF8);
        }
        public static string HTTP_GET(string url, Encoding Encode)
        {
            string result = string.Empty;
            try
            {
                var client = new WebClient();
                client.Encoding = Encode;
                result = client.DownloadString(url);
            }
            catch (Exception ex)
            {
                result = "ERR:" + ex.Message;
            }
            return result;
        }

        #endregion

        #region WebClient POST
        public static string HTTP_POST(string url, string data)
        {
            return HTTP_POST(url, data, Encoding.UTF8);
        }
        /// <summary>
        /// post方式请求
        /// </summary>
        /// <param name="url">请求路径</param>
        /// <param name="postString">请求数据</param>
        /// <param name="ContentType">默认application/x-www-form-urlencoded</param>
        /// <param name="Encode">编码:1 UTF8  2 ASCII  其他 Default   默认UTF8</param>
        /// <returns></returns>
        public static string HTTP_POST(string url, string data, Encoding Encode, string ContentType = "application/x-www-form-urlencoded")
        {
            string result = string.Empty;
            try
            {
                byte[] postData = Encode.GetBytes(data);
                WebClient webClient = new WebClient();
                webClient.Headers.Add("Content-Type", ContentType);//采取POST方式必须加的header
                byte[] responseData = webClient.UploadData(url, "POST", postData);//得到返回字符流  
                result = Encode.GetString(responseData); ;//解码
            }
            catch (Exception ex)
            {
                result = "ERR:" + ex.Message;
            }
            return result;
        }

        #endregion


        #region 微信证书post请求
        /// <summary>
        /// 微信证书post请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string WxCerHttpPost(string url, string param)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "*/*";
            request.Timeout = 15000;
            request.AllowAutoRedirect = false;

            ServicePointManager.ServerCertificateValidationCallback = new
            RemoteCertificateValidationCallback(CheckValidationResult);
            X509Certificate cer = new X509Certificate(PayConfig.CerPath, PayConfig.GetMerchantNo(Model.PayParamType.WeChatPay));
            request.ClientCertificates.Add(cer);

            StreamWriter requestStream = null;
            WebResponse response = null;
            string responseStr = null;

            try
            {
                requestStream = new StreamWriter(request.GetRequestStream());
                requestStream.Write(param);
                requestStream.Close();

                response = request.GetResponse();
                if (response != null)
                {
                    StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                    responseStr = reader.ReadToEnd();
                    reader.Close();
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                request = null;
                requestStream = null;
                response = null;
            }

            return responseStr;
        }

        private static bool CheckValidationResult(object sender,
X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            //if (errors == SslPolicyErrors.None)
            return true;
            //return false;
        }
        #endregion


        /// <summary>
        /// 获取 签名 MD5加密字符串
        /// </summary>
        /// <param name="sd"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string CreateSign(SortedDictionary<string, object> sd, string key)
        {
            Dictionary<string, object> dic = sd.OrderBy(d => d.Key).ToDictionary(d => d.Key, d => d.Value);
            var sign = dic.Aggregate("", (current, d) => current + (d.Key + "=" + d.Value + "&"));
            sign += "key=" + key;
            return MD5Util.GetMD5(sign, "UTF-8");
        }

        /// <summary>  
        /// 获取时间戳  
        /// </summary>  
        /// <returns></returns>  
        public static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }
        public static string getNoncestr()
        {
            Random random = new Random();
            return MD5Util.GetMD5(random.Next(1000).ToString(), "UTF-8");
        }

        /// <summary>
        /// 生成微信 字典排序的 xml格式字符串
        /// </summary>
        /// <param name="sdo">C#内置对象，默认排序</param>
        /// <returns></returns>
        public static string WeChatSignXml(SortedDictionary<string, object> sdo)
        {
            StringBuilder sXML = new StringBuilder("<xml>");
            foreach (var dr in sdo)
            {
                sXML.AppendFormat("<{0}>{1}</{0}>", dr.Key, dr.Value);
            }
            sXML.Append("</xml>");
            return sXML.ToString();
        }
    }
}
