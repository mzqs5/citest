using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Configuration;
using Abp.Zero.Configuration;
using DevExtreme.AspNet.Mvc;
using IF.Authorization.Users;
using IF.Porsche;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.AspNetCore.Mvc;
using Abp.Application.Services.Dto;
using Abp;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Authorization;
using IF.Authorization;
using System.ComponentModel.DataAnnotations;
using Essensoft.AspNetCore.Payment.WeChatPay;
using Microsoft.Extensions.Options;
using Essensoft.AspNetCore.Payment.WeChatPay.Request;
using Microsoft.AspNetCore.Http;
using Essensoft.AspNetCore.Payment.WeChatPay.Notify;
using IF.Configuration;
using Newtonsoft.Json;

namespace IF.Pay
{
    public class WeChatPayAppService : IFAppServiceBase, IWeChatPayAppService
    {
        private readonly IWeChatPayClient _client;
        private readonly IOptions<WeChatPayOptions> _optionsAccessor;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IWeChatPayNotifyClient PayNotifyClient;
        IRepository<OrderAggregate> OrderRepository;
        IRepository<User, long> UserRepository;
        IRepository<StoreWxPayAggregate> StoreWxPayRepository;
        public WeChatPayAppService(IWeChatPayClient client,
            IOptions<WeChatPayOptions> optionsAccessor,
            IRepository<OrderAggregate> OrderRepository,
            IHttpContextAccessor httpContextAccessor,
            IWeChatPayNotifyClient PayNotifyClient,
            IRepository<User,long> UserRepository,
            IRepository<StoreWxPayAggregate> StoreWxPayRepository)
        {
            _client = client;
            _optionsAccessor = optionsAccessor;
            this.OrderRepository = OrderRepository;
            this.httpContextAccessor = httpContextAccessor;
            this.PayNotifyClient = PayNotifyClient;
            this.UserRepository = UserRepository;
            this.StoreWxPayRepository = StoreWxPayRepository;
        }
        /// <summary>
        /// JSAPI支付
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<object> JSAPIPay(int id, string openId)
        {
            var Order = await OrderRepository.GetAll().Include(p => p.OrderItems).FirstOrDefaultAsync(p => p.Id == id);
            if (Order == null)
                throw new AbpException("未找到该订单");
            var StoreWxPay = await StoreWxPayRepository.GetAll().FirstOrDefaultAsync(p => p.StoreId == Order.DealerId);
            _optionsAccessor.Value.AppId = StoreWxPay.appid;
            _optionsAccessor.Value.MchId = StoreWxPay.mch_id;
            _optionsAccessor.Value.Key = StoreWxPay.key;
            if (StoreWxPay == null)
                throw new AbpException("未找到支付商户配置");
            var request = new WeChatPayUnifiedOrderRequest
            {
                Body = Order.OrderItems.FirstOrDefault().ProductName,
                OutTradeNo = Order.OrderNo,
                TotalFee = (int)(Order.Amount * 100),
                SpBillCreateIp = this.httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                NotifyUrl = AppConfigurations.GetAppSettings().GetSection("FileService").Value + "api/services/app/WeChatPay/Unifiedorder",
                TradeType = "JSAPI",
                OpenId = openId
            };
            var response = await _client.ExecuteAsync(request, _optionsAccessor.Value);
            if (response.ReturnCode == "SUCCESS" && response.ResultCode == "SUCCESS")
            {
                var req = new WeChatPayJsApiSdkRequest
                {
                    Package = "prepay_id=" + response.PrepayId
                };
                var parameter = await _client.ExecuteAsync(req, _optionsAccessor.Value);
                // 将参数(parameter)给 公众号前端 让他在微信内H5调起支付(https://pay.weixin.qq.com/wiki/doc/api/jsapi.php?chapter=7_7&index=6)
                return new { PayParameter = JsonConvert.SerializeObject(parameter), ResponseBody = response.ResponseBody };
            }
            return response;
        }

        /// <summary>
        /// 扫码支付
        /// </summary>
        /// <param name="id">订单Id</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<object> QrCodePay(int id)
        {
            var Order = await OrderRepository.GetAll().Include(p => p.OrderItems).FirstOrDefaultAsync(p => p.Id == id);
            if (Order == null)
                throw new AbpException("未找到该订单");
            var StoreWxPay = await StoreWxPayRepository.GetAll().FirstOrDefaultAsync(p => p.StoreId == Order.DealerId);
            _optionsAccessor.Value.AppId = StoreWxPay.appid;
            _optionsAccessor.Value.MchId = StoreWxPay.mch_id;
            _optionsAccessor.Value.Key = StoreWxPay.key;
            if (StoreWxPay == null)
                throw new AbpException("未找到支付商户配置");
            var request = new WeChatPayUnifiedOrderRequest
            {
                Body = Order.OrderItems.FirstOrDefault().ProductName,
                OutTradeNo = Order.OrderNo,
                TotalFee = (int)(Order.Amount * 100),
                SpBillCreateIp = this.httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                NotifyUrl = AppConfigurations.GetAppSettings().GetSection("FileService").Value + "api/services/app/WeChatPay/Unifiedorder",
                TradeType = "NATIVE"

            };
            var response = await _client.ExecuteAsync(request, _optionsAccessor.Value);
            // response.CodeUrl 给前端生成二维码
            return new { qrcode = response.CodeUrl, response = response.ResponseBody, Amount = Order.Amount };
        }

        /// <summary>
        /// 小程序支付
        /// </summary>
        /// <param name="id">订单id</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<object> LiteAppPay(int id)
        {
            var Order = await OrderRepository.GetAll().Include(p => p.OrderItems).FirstOrDefaultAsync(p => p.Id == id);
            if (Order == null)
                throw new AbpException("未找到该订单");
            var StoreWxPay = await StoreWxPayRepository.GetAll().FirstOrDefaultAsync(p => p.StoreId == Order.DealerId);//
            _optionsAccessor.Value.AppId = StoreWxPay.appid;
            _optionsAccessor.Value.MchId = StoreWxPay.mch_id;
            _optionsAccessor.Value.Key = StoreWxPay.key;
            if (StoreWxPay == null)
                throw new AbpException("未找到支付商户配置");
            var user = await UserRepository.GetAll().Include(p => p.Wechats).FirstOrDefaultAsync(p => p.Id == Order.UserId);
            if (user == null)
                throw new AbpException("未找到该用户！");
            var wechat = user.Wechats.FirstOrDefault(p => p.Type == 2);
            if (wechat == null)
                throw new AbpException("该用户未绑定小程序openid！");
            var request = new WeChatPayUnifiedOrderRequest
            {
                Body = Order.OrderItems.FirstOrDefault().ProductName,
                OutTradeNo = Order.OrderNo,
                TotalFee = (int)(Order.Amount * 100),
                SpBillCreateIp = this.httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                NotifyUrl = AppConfigurations.GetAppSettings().GetSection("FileService").Value + "api/services/app/WeChatPay/Unifiedorder",
                TradeType = "JSAPI",
                OpenId = wechat.OpenId
            };
            var response = await _client.ExecuteAsync(request, _optionsAccessor.Value);

            if (response.ReturnCode == "SUCCESS" && response.ResultCode == "SUCCESS")
            {
                var req = new WeChatPayLiteAppSdkRequest
                {
                    Package = "prepay_id=" + response.PrepayId
                };
                var parameter = await _client.ExecuteAsync(req, _optionsAccessor.Value);
                // 将参数(parameter)给 小程序前端 让他调起支付API(https://pay.weixin.qq.com/wiki/doc/api/wxa/wxa_api.php?chapter=7_7&index=5)
                return new { parameter = parameter, response = response.ResponseBody, Amount = Order.Amount };
            }
            else
                throw new AbpException($"获取支付报文错误！{JsonConvert.SerializeObject(response)}");

        }

        /// <summary>
        /// H5支付
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<object> H5Pay(int id)
        {
            var Order = await OrderRepository.GetAll().Include(p => p.OrderItems).FirstOrDefaultAsync(p => p.Id == id);
            if (Order == null)
                throw new AbpException("未找到该订单");
            var StoreWxPay = await StoreWxPayRepository.GetAll().FirstOrDefaultAsync(p => p.StoreId == Order.DealerId);
            _optionsAccessor.Value.AppId = StoreWxPay.appid;
            _optionsAccessor.Value.MchId = StoreWxPay.mch_id;
            _optionsAccessor.Value.Key = StoreWxPay.key;
            if (StoreWxPay == null)
                throw new AbpException("未找到支付商户配置");
            var request = new WeChatPayUnifiedOrderRequest
            {
                Body = Order.OrderItems.FirstOrDefault().ProductName,
                OutTradeNo = Order.OrderNo,
                TotalFee = (int)(Order.Amount * 100),
                SpBillCreateIp = this.httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                NotifyUrl = AppConfigurations.GetAppSettings().GetSection("FileService").Value + "api/services/app/WeChatPay/Unifiedorder",
                TradeType = "MWEB"
            };
            var response = await _client.ExecuteAsync(request, _optionsAccessor.Value);

            // mweb_url为拉起微信支付收银台的中间页面，可通过访问该url来拉起微信客户端，完成支付,mweb_url的有效期为5分钟。
            return response;
        }

        /// <summary>
        /// 统一下单支付结果通知
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Unifiedorder()
        {
            try
            {
                var notify = await PayNotifyClient.ExecuteAsync<WeChatPayUnifiedOrderNotify>(httpContextAccessor.HttpContext.Request, _optionsAccessor.Value);
                if (notify.ReturnCode == "SUCCESS")
                {
                    if (notify.ResultCode == "SUCCESS")
                    {
                        Console.WriteLine("OutTradeNo: " + notify.OutTradeNo);
                        var Order = await OrderRepository.GetAllIncluding(p => p.OrderItems).FirstOrDefaultAsync(p => p.OrderNo.Equals(notify.OutTradeNo, StringComparison.CurrentCultureIgnoreCase));
                        Order.State = 1;
                        Order.OrderItems.ForEach(item =>
                        {
                            int day = 0;
                            int.TryParse(item.Validity, out day);
                            item.CouponETime = DateTime.Now.AddDays(day);
                        });
                        await OrderRepository.UpdateAsync(Order);
                        await CurrentUnitOfWork.SaveChangesAsync();
                        return WeChatPayNotifyResult.Success;
                    }
                }
                return new NoContentResult();
            }
            catch
            {
                return new NoContentResult();
            }
        }

        /// <summary>
        /// 退款结果通知
        /// </summary>
        /// <returns></returns>
        [Route("refund")]
        [HttpPost]
        public async Task<IActionResult> Refund()
        {
            try
            {
                var notify = await PayNotifyClient.ExecuteAsync<WeChatPayRefundNotify>(httpContextAccessor.HttpContext.Request, _optionsAccessor.Value);
                if (notify.ReturnCode == "SUCCESS")
                {
                    if (notify.RefundStatus == "SUCCESS")
                    {
                        Console.WriteLine("OutTradeNo: " + notify.OutTradeNo);
                        return WeChatPayNotifyResult.Success;
                    }
                }
                return new NoContentResult();
            }
            catch
            {
                return new NoContentResult();
            }
        }
    }

    public class LiteAppPayViewModel
    {
        [Required]
        [Display(Name = "订单Id")]
        public int Id { get; set; }

        [Required]
        [Display(Name = "小程序openId")]
        public string OpenId { get; set; }
    }

    public class WeChatPayQrCodePayViewModel
    {
        [Required]
        [Display(Name = "out_trade_no")]
        public string OutTradeNo { get; set; }

        [Required]
        [Display(Name = "body")]
        public string Body { get; set; }

        [Required]
        [Display(Name = "total_fee")]
        public int TotalFee { get; set; }

        [Required]
        [Display(Name = "spbill_create_ip")]
        public string SpBillCreateIp { get; set; }

        [Required]
        [Display(Name = "notify_url")]
        public string NotifyUrl { get; set; }

        [Required]
        [Display(Name = "trade_type")]
        public string TradeType { get; set; }
    }
}
