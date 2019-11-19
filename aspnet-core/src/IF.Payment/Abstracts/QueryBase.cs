using IF.Payment.Model;
using Microsoft.AspNetCore.Http;

namespace IF.Payment.Abstracts
{
    /// <summary>
    /// 查询基础类
    /// </summary>
    public class QueryBase
    {
        protected QueryBase() { }

        public string order_no { get; set; }

        public PayType payType { get; set; }

        public HttpContext httpContext { get; set; }

    }

}
