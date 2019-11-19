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
using DbHelper;
using IF.Configuration;
using Essensoft.AspNetCore.Payment.WeChatPay;
using Microsoft.Extensions.Options;
using Essensoft.AspNetCore.Payment.WeChatPay.Request;
using Microsoft.AspNetCore.Http;
using IF.Common;
using DevExtreme.AspNet.Data;

namespace IF.Porsche
{
    /// <summary>
    /// 订单使用应用服务
    /// </summary>
    public class OrderUseAppService : IFAppServiceBase, IOrderUseAppService
    {
        IRepository<OrderAggregate> OrderRepository;
        IRepository<OrderUseAggregate> OrderUseRepository;
        IRepository<StoreWxPayAggregate> StoreWxPayRepository;
        IRepository<User, long> UserRepository;
        IRepository<DealerAggregate> DealerRepository;
        MSSqlHelper sqlHelper;
        private readonly IWeChatPayClient _client;
        private readonly IOptions<WeChatPayOptions> _optionsAccessor;
        private readonly IHttpContextAccessor httpContextAccessor;
        public OrderUseAppService(
            IRepository<OrderAggregate> OrderRepository,
            IRepository<OrderUseAggregate> OrderUseRepository,
            IRepository<User, long> UserRepository,
            IRepository<DealerAggregate> DealerRepository,
            IWeChatPayClient client,
            IOptions<WeChatPayOptions> optionsAccessor,
            IHttpContextAccessor httpContextAccessor,
            IRepository<StoreWxPayAggregate> StoreWxPayRepository)
        {
            this.OrderRepository = OrderRepository;
            sqlHelper = new MSSqlHelper(AppConfigurations.GetAppSettings().GetSection("ConnectionStrings").GetSection("OutSide").Value);
            _client = client;
            _optionsAccessor = optionsAccessor;
            this.httpContextAccessor = httpContextAccessor;
            this.StoreWxPayRepository = StoreWxPayRepository;
            this.UserRepository = UserRepository;
            this.DealerRepository = DealerRepository;
            this.OrderUseRepository = OrderUseRepository;
        }

        #region 获取订单使用的相关信息，支持分页查询
        /// <summary>
        /// 获取订单使用的相关信息，支持分页查询（后台使用）
        /// </summary>
        /// <param name="loadOptions">查询参数</param>
        /// <returns></returns>
        public async Task<object> GetListDataAsync(DataSourceLoadOptions loadOptions)
        {
            var Dealers = sqlHelper.Query<DealerDto>(
                string.Format(@"select Id,REPLACE (Name,'-PMI','') as Name,Distance,Address,Phone,Status,Longitude,Latitude,AUserId,DDesc,System_PlaceId from Dealer
                           where Name like '%-PMI'"));
            var result = from OrderUse in OrderUseRepository.GetAll().AsEnumerable()
                         join Order in OrderRepository.GetAll().Include(p => p.OrderItems).AsEnumerable()
                         on OrderUse.OrderId equals Order.Id
                         join User in UserRepository.GetAll().AsEnumerable()
                         on Order.UserId equals User.Id
                         join Dealer in Dealers
                         on Order.DealerId equals Dealer.Id
                         select new OrderUseDto
                         {
                             OrderNo = Order.OrderNo,
                             UserId = Order.UserId,
                             DealerId = Order.DealerId,
                             StoreName = Dealer.Name,
                             FullName = User.Surname + User.Name,
                             Mobile = User.Mobile,
                             Amount = Order.Amount,
                             Id = OrderUse.Id,
                             State = OrderUse.State,
                             Remarks = OrderUse.Remarks,
                             CreationTime = OrderUse.CreationTime,
                             PayAmount = OrderUse.PayAmount,
                             GoodsName = OrderUse.GoodsName,
                             WorkNo = OrderUse.WorkNo,
                             OrderId = OrderUse.OrderId,
                             Balance = OrderUse.Balance,
                             ETime = OrderUse.ETime,
                             OrderItems = this.ObjectMapper.Map<List<OrderItemDto>>(Order.OrderItems)
                         };
            loadOptions.Sort = new SortingInfo[] { new SortingInfo() { Selector = "CreationTime", Desc = true } };
            return base.DataSourceLoadMap(result, loadOptions);
        }

        #endregion

        #region 根据ID获取订单信息
        /// <summary>
        /// 根据ID获取订单信息
        /// </summary>
        /// <param name="id">订单信息主键</param>
        /// <returns></returns>
        public async Task<OrderUseDto> GetAsync(int id)
        {
            try
            {
                var Order = await OrderUseRepository.GetAll().Where(k => k.Id == id).FirstOrDefaultAsync();

                if (Order == null)
                {
                    throw new EntityNotFoundException(typeof(OrderUseAggregate), id);
                }
                var dto = this.ObjectMapper.Map<OrderUseDto>(Order);
                return dto;
            }
            catch (Exception e)
            {
                Logger.Error("订单获取异常！", e);
                throw new AbpException("订单获取异常", e);
            }
        }
        #endregion

        #region  新增订单使用信息
        /// <summary>
        /// 新增订单使用信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [AbpAuthorize("Admin.OrderUse.Edit")]
        public async Task SaveAsync(OrderUseEditDto input)
        {
            if (!AbpSession.UserId.HasValue)
                throw new AbpException("请先登录！");
            OrderUseAggregate entity = this.ObjectMapper.Map<OrderUseAggregate>(input);

            OrderAggregate Order = OrderRepository.GetAll().Where(k => k.Id == entity.OrderId).FirstOrDefault();
            Order.Progress = 1;
            await OrderRepository.UpdateAsync(Order);
            entity.Balance = Order.Balance;
            await OrderUseRepository.InsertAsync(entity);
            await CurrentUnitOfWork.SaveChangesAsync();
        }
        #endregion

        #region 审批
        /// <summary>
        /// 审批
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AbpAuthorize("Admin.OrderUse.WriteOff")]
        public async Task WriteOff(string ids)
        {
            foreach (var id in ids.Split(','))
            {
                var entity = await OrderUseRepository.FirstOrDefaultAsync(p => p.Id == int.Parse(id));
                entity.State = 1;

                OrderAggregate Order = OrderRepository.GetAll().Where(k => k.Id == entity.OrderId).FirstOrDefault();
                if (Order.Balance < entity.PayAmount)
                    throw new AbpException("余额不足！");
                Order.Balance -= entity.PayAmount;
                if (Order.Balance == 0)
                    Order.Progress = 2;
                entity.Balance = Order.Balance;
                await OrderUseRepository.UpdateAsync(entity);
                await OrderRepository.UpdateAsync(Order);
            }
            await CurrentUnitOfWork.SaveChangesAsync();
        }
        #endregion

        #region 驳回
        /// <summary>
        /// 驳回
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AbpAuthorize("Admin.OrderUse.WriteOff")]
        public async Task Reject(string ids)
        {
            foreach (var id in ids.Split(','))
            {
                var entity = await OrderUseRepository.FirstOrDefaultAsync(p => p.Id == int.Parse(id));
                entity.State = 2;
                await OrderUseRepository.UpdateAsync(entity);
            }
            await CurrentUnitOfWork.SaveChangesAsync();
        }
        #endregion

    }

    public class OrderUseEditDto : EntityDto
    {

        /// <summary>
        /// 订单Id
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// 有效期
        /// </summary>
        public DateTime ETime { get; set; }

        /// <summary>
        /// 消费项目
        /// </summary>
        public string GoodsName { get; set; }

        /// <summary>
        /// 消费金额
        /// </summary>
        public decimal PayAmount { get; set; }

        /// <summary>
        /// 工单号
        /// </summary>
        public string WorkNo { get; set; }

        /// <summary>
        /// 状态 0待核销/1已使用/2已驳回
        /// </summary>
        //public int State { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; }

    }

    public class OrderUseDto : EntityDto
    {
        /// <summary>
        /// 会员Id
        /// </summary>
        public long UserId { get; set; }
        /// <summary>
        /// 经销商Id
        /// </summary>
        public int DealerId { get; set; }

        /// <summary>
        /// 全名
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 门店名称
        /// </summary>
        public string StoreName { get; set; }
        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderNo { get; set; }

        public DateTime CreationTime { get; set; }

        /// <summary>
        /// 订单Id
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// 有效期
        /// </summary>
        public DateTime ETime { get; set; }

        /// <summary>
        /// 消费项目
        /// </summary>
        public string GoodsName { get; set; }

        /// <summary>
        /// 订单总金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 余额
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// 消费金额
        /// </summary>
        public decimal PayAmount { get; set; }

        /// <summary>
        /// 工单号
        /// </summary>
        public string WorkNo { get; set; }

        /// <summary>
        /// 状态 0待审批/1已使用/2已驳回
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string StateText
        {
            get
            {
                if (this.State == 1)
                    return "已使用";
                else if (this.State == 2)
                    return "已驳回";
                else
                    return "待审批";
            }
        }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// 订单详细
        /// </summary>
        public List<OrderItemDto> OrderItems { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName
        {
            get
            {
                var item = string.Join(",", this.OrderItems.Select(p => p.ProductName).ToArray());
                return item;
            }
        }

        /// <summary>
        /// 车型id
        /// </summary>
        public string CModelIds
        {
            get
            {
                var item = string.Join(",", this.OrderItems.Select(p => p.CModelIds).Distinct());
                return item;
            }
        }

        /// <summary>
        /// 车型
        /// </summary>
        public string CModels
        {
            get
            {
                var item = string.Join(",", this.OrderItems.Select(p => p.CModels).Distinct());
                return item;
            }
        }
    }
}
