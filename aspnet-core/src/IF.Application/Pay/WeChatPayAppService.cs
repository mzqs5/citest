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
        /// JSAPI֧��
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<object> JSAPIPay(int id, string openId)
        {
            var Order = await OrderRepository.GetAll().Include(p => p.OrderItems).FirstOrDefaultAsync(p => p.Id == id);
            if (Order == null)
                throw new AbpException("δ�ҵ��ö���");
            var StoreWxPay = await StoreWxPayRepository.GetAll().FirstOrDefaultAsync(p => p.StoreId == Order.DealerId);
            _optionsAccessor.Value.AppId = StoreWxPay.appid;
            _optionsAccessor.Value.MchId = StoreWxPay.mch_id;
            _optionsAccessor.Value.Key = StoreWxPay.key;
            if (StoreWxPay == null)
                throw new AbpException("δ�ҵ�֧���̻�����");
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
                // ������(parameter)�� ���ں�ǰ�� ������΢����H5����֧��(https://pay.weixin.qq.com/wiki/doc/api/jsapi.php?chapter=7_7&index=6)
                return new { PayParameter = JsonConvert.SerializeObject(parameter), ResponseBody = response.ResponseBody };
            }
            return response;
        }

        /// <summary>
        /// ɨ��֧��
        /// </summary>
        /// <param name="id">����Id</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<object> QrCodePay(int id)
        {
            var Order = await OrderRepository.GetAll().Include(p => p.OrderItems).FirstOrDefaultAsync(p => p.Id == id);
            if (Order == null)
                throw new AbpException("δ�ҵ��ö���");
            var StoreWxPay = await StoreWxPayRepository.GetAll().FirstOrDefaultAsync(p => p.StoreId == Order.DealerId);
            _optionsAccessor.Value.AppId = StoreWxPay.appid;
            _optionsAccessor.Value.MchId = StoreWxPay.mch_id;
            _optionsAccessor.Value.Key = StoreWxPay.key;
            if (StoreWxPay == null)
                throw new AbpException("δ�ҵ�֧���̻�����");
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
            // response.CodeUrl ��ǰ�����ɶ�ά��
            return new { qrcode = response.CodeUrl, response = response.ResponseBody, Amount = Order.Amount };
        }

        /// <summary>
        /// С����֧��
        /// </summary>
        /// <param name="id">����id</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<object> LiteAppPay(int id)
        {
            var Order = await OrderRepository.GetAll().Include(p => p.OrderItems).FirstOrDefaultAsync(p => p.Id == id);
            if (Order == null)
                throw new AbpException("δ�ҵ��ö���");
            var StoreWxPay = await StoreWxPayRepository.GetAll().FirstOrDefaultAsync(p => p.StoreId == Order.DealerId);//
            _optionsAccessor.Value.AppId = StoreWxPay.appid;
            _optionsAccessor.Value.MchId = StoreWxPay.mch_id;
            _optionsAccessor.Value.Key = StoreWxPay.key;
            if (StoreWxPay == null)
                throw new AbpException("δ�ҵ�֧���̻�����");
            var user = await UserRepository.GetAll().Include(p => p.Wechats).FirstOrDefaultAsync(p => p.Id == Order.UserId);
            if (user == null)
                throw new AbpException("δ�ҵ����û���");
            var wechat = user.Wechats.FirstOrDefault(p => p.Type == 2);
            if (wechat == null)
                throw new AbpException("���û�δ��С����openid��");
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
                // ������(parameter)�� С����ǰ�� ��������֧��API(https://pay.weixin.qq.com/wiki/doc/api/wxa/wxa_api.php?chapter=7_7&index=5)
                return new { parameter = parameter, response = response.ResponseBody, Amount = Order.Amount };
            }
            else
                throw new AbpException($"��ȡ֧�����Ĵ���{JsonConvert.SerializeObject(response)}");

        }

        /// <summary>
        /// H5֧��
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<object> H5Pay(int id)
        {
            var Order = await OrderRepository.GetAll().Include(p => p.OrderItems).FirstOrDefaultAsync(p => p.Id == id);
            if (Order == null)
                throw new AbpException("δ�ҵ��ö���");
            var StoreWxPay = await StoreWxPayRepository.GetAll().FirstOrDefaultAsync(p => p.StoreId == Order.DealerId);
            _optionsAccessor.Value.AppId = StoreWxPay.appid;
            _optionsAccessor.Value.MchId = StoreWxPay.mch_id;
            _optionsAccessor.Value.Key = StoreWxPay.key;
            if (StoreWxPay == null)
                throw new AbpException("δ�ҵ�֧���̻�����");
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

            // mweb_urlΪ����΢��֧������̨���м�ҳ�棬��ͨ�����ʸ�url������΢�ſͻ��ˣ����֧��,mweb_url����Ч��Ϊ5���ӡ�
            return response;
        }

        /// <summary>
        /// ͳһ�µ�֧�����֪ͨ
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
        /// �˿���֪ͨ
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
        [Display(Name = "����Id")]
        public int Id { get; set; }

        [Required]
        [Display(Name = "С����openId")]
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
