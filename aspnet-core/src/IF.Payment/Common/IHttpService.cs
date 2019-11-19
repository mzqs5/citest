using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IF.Payment.Common
{
    public interface IHttpService
    {
        Encoding encoding { get; set; }
        /// <summary>
        /// 字符编码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        Task<string> UrlEncode(string str);

        /// <summary>
        /// 字符解码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        Task<string> UrlDecode(string str);

        /// <summary>
        /// HTTP POST方式请求数据
        /// </summary>
        /// <param name="url">URL.</param>
        /// <param name="param">POST的数据</param>
        /// <returns></returns>
        Task<string> HttpPost(string url, string param);

        /// <summary>
        /// HTTP GET方式请求数据.
        /// </summary>
        /// <param name="url">URL.</param>
        /// <returns></returns>
        Task<string> HttpGet(string url);

        /// <summary>
        /// 执行URL获取页面内容
        /// </summary>
        Task<string> UrlExecute(string urlPath);
    }
}
