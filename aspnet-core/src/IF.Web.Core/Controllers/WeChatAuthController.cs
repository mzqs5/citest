using Abp.Domain.Repositories;
using IF.Configuration;
using IF.Porsche;
using Microsoft.AspNetCore.Mvc;
using Senparc.Weixin;
using Senparc.Weixin.MP.AdvancedAPIs;
using System;
using System.Collections.Generic;
using System.Text;

namespace IF.Controllers
{
    [Route("api/[controller]/[action]")]
    public class WeChatAuthController : IFControllerBase
    {
        IRepository<StoreWxPayAggregate> StoreWxPayRepository;
        public WeChatAuthController(IRepository<StoreWxPayAggregate> StoreWxPayRepository)
        {
            this.StoreWxPayRepository = StoreWxPayRepository;
        }
        // GET: OAuth
        public ActionResult GetOpenId(int storeId, string returnUrl)
        {
            string redirect_uri = $"{AppConfigurations.GetAppSettings().GetSection("FileService").Value}{Url.Action("CallBack", new { returnUrl = returnUrl })}";
            //3.state 也是下面的方法需要的参数，这个参数是可以自定义的
            string state = $"wx_{storeId}_" + DateTime.Now.Millisecond;
            //1.OAuthApi下的GetAuthorizeUrl方法用来获取验证地址,第二个参数是redirect_uri，所以我们就需要构造这个参数
            var wxconfig = StoreWxPayRepository.FirstOrDefault(p => p.StoreId == storeId);
            if (wxconfig == null)
                return Content("未配置微信公众号");
            string redirect = OAuthApi.GetAuthorizeUrl(wxconfig.public_appid, redirect_uri, state, Senparc.Weixin.MP.OAuthScope.snsapi_base);
            return Redirect(redirect);
        }
        ////为什么需要CallBack，在获取到用户的授权之后，需要获取用户的code
        public ActionResult CallBack(string code, string state, string returnUrl)
        {
            if (!state.Contains("wx"))
            {
                return Content("state参数错误，请重新进入。");
            }
            if (state.Split('_').Length != 2)
            {
                return Content("state参数错误，请重新进入。");
            }
            var storeId = int.Parse(state.Split('_')[1]);
            //如果code返回的是个空值，则需要回到授权界面，重新授权
            if (string.IsNullOrEmpty(code))
            {
                return RedirectToAction("index");
            }
            var wxconfig = StoreWxPayRepository.FirstOrDefault(p => p.StoreId == storeId);
            if (wxconfig == null)
                return Content("未配置微信公众号");
            //通过回调函数返回的code来获取令牌  ，如果不懂可单步执行，看url的变化
            var accessToken = OAuthApi.GetAccessToken(wxconfig.public_appid, wxconfig.public_secret, code);//这里返回的是一个对象，可以用弱类型var接收
            if (accessToken.errcode != ReturnCode.请求成功)
            {
                //如果令牌的错误信息不等于请求成功，则需要重新返回授权界面
                return Content(accessToken.errmsg);
            }
            if (returnUrl.Contains("#"))
            {
                returnUrl = returnUrl.Substring(0, returnUrl.IndexOf("#"));
            }
            return Redirect(returnUrl + (returnUrl.Contains("?") ? "&" : "?") + $"openId={accessToken.openid}");
            //var userinfo = OAuthApi.GetUserInfo(accessToken.access_token, accessToken.openid);
        }
    }
}
