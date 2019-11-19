using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using IF.Payment.Abstracts;
using IF.Payment.Attributes;
using IF.Payment.Common;
using IF.Payment.Model;

namespace IF.Payment
{
    public class PaymentManager
    {
        readonly IHttpContextAccessor httpContextAccessor;
        public PaymentManager(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }
        private Dictionary<PayType, PayBase> dic = new Dictionary<PayType, PayBase>();

        /// <summary>
        /// 预支付，获取支付链接
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<PayRst> PaymentRequest(QueryBase query)
        {
            PayBase payBase;
            PayTypeAttribute pay = EnumService.GetAttribute<PayTypeAttribute>(query.payType) as PayTypeAttribute;
            if (!dic.TryGetValue(query.payType, out payBase))
            {
                try
                {
                    var assmbly = Assembly.GetExecutingAssembly();
                    payBase = assmbly.CreateInstance(pay.NameSpaces + "." + pay.ClassName) as PayBase;
                    dic[query.payType] = payBase;
                }
                catch (DllNotFoundException) { throw; }
                catch (Exception) { throw; }

            }

            PayRst rst = payBase.Execute(query);
            return rst;

        }
        /// <summary>
        /// 唤醒支付
        /// </summary>
        /// <param name="content"></param>
        /// <param name="customHandle">自定义处理，如果不为空，直接用该委托处理</param>
        public async Task Success(string content, Action<string> customHandle)
        {
            if (customHandle != null)
            {
                customHandle(content);
                return;
            }
            if (content.ToLower().Contains("form"))
                await httpContextAccessor.HttpContext.Response.WriteAsync(content);
            else
                await httpContextAccessor.HttpContext.Response.WriteAsync("<script>window.location.href='" + content + "'</script>");
        }

        /// <summary>
        /// 唤醒支付
        /// </summary>
        /// <param name="content"></param>
        /// <param name="customHandle">自定义处理，如果不为空，直接用该委托处理</param>
        public async Task<string> SuccessResultStr(string content, Action<string> customHandle)
        {
            if (customHandle != null)
            {
                customHandle(content);
                return content;
            }
            if (content.ToLower().Contains("form"))
                return content;
            else
                return "<script>window.location.href='" + content + "'</script>";
        }

    }
}
