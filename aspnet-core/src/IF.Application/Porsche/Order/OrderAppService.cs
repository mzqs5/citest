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
using Abp.Linq.Extensions;

namespace IF.Porsche
{
    /// <summary>
    /// 订单应用服务
    /// </summary>
    public class OrderAppService : IFAppServiceBase, IOrderAppService
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
        public OrderAppService(
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
            this.OrderUseRepository = OrderUseRepository;
            this.httpContextAccessor = httpContextAccessor;
            this.StoreWxPayRepository = StoreWxPayRepository;
            this.UserRepository = UserRepository;
            this.DealerRepository = DealerRepository;
        }

        #region 获取订单的相关信息，支持分页查询
        /// <summary>
        /// 获取订单的相关信息，支持分页查询（后台使用）
        /// </summary>
        /// <param name="loadOptions">查询参数</param>
        /// <returns></returns>
        public async Task<object> GetListDataAsync(DataSourceLoadOptions loadOptions)
        {
            var Dealers = sqlHelper.Query<DealerDto>(
                string.Format(@"select Id,REPLACE (Name,'-PMI','') as Name,Distance,Address,Phone,Status,Longitude,Latitude,AUserId,DDesc,System_PlaceId from Dealer
                           where Name like '%-PMI'")).ToList();
            var result = from Order in OrderRepository.GetAll().Include(p => p.OrderItems).AsEnumerable()
                         join User in UserRepository.GetAll().AsEnumerable()
                         on Order.UserId equals User.Id
                         into Users
                         from User in Users.DefaultIfEmpty()
                         join Dealer in Dealers
                         on Order.DealerId equals Dealer.Id
                         select new OrderDto
                         {
                             OrderNo = Order.OrderNo,
                             UserId = Order.UserId,
                             DealerId = Order.DealerId,
                             StoreName = Dealer?.Name,
                             FullName = User == null ? "" : User.Surname + User.Name,
                             Mobile = User == null ? "" : User.Mobile,
                             Amount = Order.Amount,
                             Id = Order.Id,
                             Quantity = Order.Quantity,
                             Balance = Order.Balance,
                             State = Order.State,
                             Remarks = Order.Remarks,
                             CreationTime = Order.CreationTime,
                             PayTime = Order.PayTime,
                             PayCer = Order.PayCer,
                             PayMode = Order.PayMode,
                             Progress = Order.Progress,
                             OrderItems = this.ObjectMapper.Map<List<OrderItemDto>>(Order.OrderItems)
                         };
            loadOptions.Sort = new SortingInfo[] { new SortingInfo() { Selector = "CreationTime", Desc = true } };
            return base.DataSourceLoadMap(result, loadOptions);
        }

        #endregion

        #region 获取我的订单的相关信息，支持分页查询
        /// <summary>
        /// 获取我的订单的相关信息，支持分页查询（前台使用）
        /// </summary>
        /// <param name="loadOptions">查询参数</param>
        /// <returns></returns>
        public async Task<object> GetMyListDataAsync(DataSourceLoadOptions loadOptions)
        {
            var result = from Order in OrderRepository.GetAll()
                         .Where(p => p.UserId == AbpSession.UserId.Value && (p.State != 0 || (p.State == 0 && p.CreationTime.AddMinutes(30) >= DateTime.Now)))
                         .Include(p => p.OrderItems).AsEnumerable()
                         select new OrderDto
                         {
                             OrderNo = Order.OrderNo,
                             UserId = Order.UserId,
                             DealerId = Order.DealerId,
                             Amount = Order.Amount,
                             Id = Order.Id,
                             Quantity = Order.Quantity,
                             State = Order.State,
                             Remarks = Order.Remarks,
                             CreationTime = Order.CreationTime,
                             Balance = Order.Balance,
                             OrderItems = this.ObjectMapper.Map<List<OrderItemDto>>(Order.OrderItems)
                         };
            loadOptions.Sort = new SortingInfo[] { new SortingInfo() { Selector = "CreationTime", Desc = true } };
            return base.DataSourceLoadMap(result, loadOptions);
        }

        #endregion

        #region 导出
        //[AbpAuthorize("Admin.Order.Export")]
        public async Task<IActionResult> Export(DataSourceLoadOptions loadOptions)
        {
            try
            {
                var result = from Order in OrderRepository.GetAll().Include(p => p.OrderItems).AsEnumerable()
                             join User in UserRepository.GetAll().AsEnumerable()
                             on Order.UserId equals User.Id
                             join Dealer in DealerRepository.GetAll().AsEnumerable()
                             on Order.DealerId equals Dealer.Id
                             select new OrderDto
                             {
                                 OrderNo = Order.OrderNo,
                                 UserId = Order.UserId,
                                 DealerId = Order.DealerId,
                                 StoreName = Dealer.Name,
                                 FullName = User.Name + User.Surname,
                                 Mobile = User.Mobile,
                                 Amount = Order.Amount,
                                 Id = Order.Id,
                                 Quantity = Order.Quantity,
                                 State = Order.State,
                                 Remarks = Order.Remarks,
                                 CreationTime = Order.CreationTime,
                                 OrderItems = this.ObjectMapper.Map<List<OrderItemDto>>(Order.OrderItems)
                             };
                loadOptions.Sort = new SortingInfo[] { new SortingInfo() { Selector = "CreationTime", Desc = true } };
                var Export = new ExportHelper();
                Export.AddColumn("OrderNo", "订单编号");
                Export.AddColumn("StoreName", "门店名称");
                Export.AddColumn("FullName", "姓名");
                Export.AddColumn("Mobile", "手机号码");
                Export.AddColumn("Amount", "金额");
                Export.AddColumn("Quantity", "数量");
                Export.AddColumn("Remarks", "备注");
                Export.AddColumn("CreationTime", "下单时间");
                var result1 = Export.ExportExcel<OrderDto>(base.DataSourceLoadMap(result, loadOptions).data);
                result1.FileDownloadName = "订单记录";
                return result1;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #endregion

        #region 根据ID获取订单信息
        /// <summary>
        /// 根据ID获取订单信息
        /// </summary>
        /// <param name="id">订单信息主键</param>
        /// <returns></returns>
        public async Task<OrderDto> GetAsync(int id)
        {
            try
            {
                var Order = await OrderRepository.GetAll().Include(p => p.OrderItems).Where(k => k.Id == id).FirstOrDefaultAsync();

                if (Order == null)
                {
                    throw new EntityNotFoundException(typeof(OrderAggregate), id);
                }
                var dto = this.ObjectMapper.Map<OrderDto>(Order);
                var User = await UserRepository.FirstOrDefaultAsync(p => p.Id == Order.UserId);
                if (User != null)
                {
                    dto.FullName = User.Surname + User.Name;
                    dto.Mobile = User.Mobile;
                }
                var Dealer = sqlHelper.Query<DealerDto>(
                string.Format(@"select Id,REPLACE (Name,'-PMI','') as Name,Distance,Address,Phone,Status,Longitude,Latitude,AUserId,DDesc,System_PlaceId from Dealer
                           where Id={0}", Order.DealerId)).FirstOrDefault();
                if (Dealer != null)
                {
                    dto.StoreName = Dealer.Name;
                    dto.StorePhone = Dealer.Phone;
                    dto.StoreAddress = Dealer.Address;
                }
                return dto;
            }
            catch (Exception e)
            {
                Logger.Error("订单获取异常！", e);
                throw new AbpException("订单获取异常", e);
            }
        }
        #endregion

        #region  新增订单信息
        /// <summary>
        /// 新增订单信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<object> SaveAsync(OrderEditDto input)
        {
            try
            {
                if (!AbpSession.UserId.HasValue)
                    throw new AbpException("请先登录！");
                if (input.DealerId == 0)
                    throw new AbpException("请选择经销商！");
                if (input.OrderItems.Count == 0)
                    throw new AbpException("缺少订单购买商品明细！");
                OrderAggregate entity = this.ObjectMapper.Map<OrderAggregate>(input);
                entity.UserId = AbpSession.UserId.Value;

                entity.OrderNo = AutoCodeHelper.Create("A");
                for (int i = 0; i < entity.OrderItems.Count(); i++)
                {
                    var orderItem = entity.OrderItems[i];
                    var Products = sqlHelper.Query<GoodsDto>($"select ProductId,ProductName,ProductPic,ProductDetailPic,ProductSales,ProductPrice,DisplayPrice,ProductDesc,Processing,Mask,CouponId,DealerId,PriceCount,ProductStatus,IsTop,Name as CouponName,ETime as CouponETime,Validity as CouponValidity,[Desc] as CouponDesc,Tags as CouponTags,Money as CouponMoney,Blance as CouponBlance,UseNumber as CouponUseNumber,CModelsId,Status as CouponStatus from Products as product left join Coupon as coupon on product.CouponId = coupon.id where product.ProductId ={ orderItem.ProductId}");
                    var Product = Products.FirstOrDefault();
                    orderItem.ProductName = Product.ProductName;
                    orderItem.ProductPic = Product.ProductPic;
                    orderItem.CModelIds = Product.CModelsId;
                    if (Product.CModelsId != ",0,")
                    {
                        var CModels = sqlHelper.Query<CModelAggregate>($"select * from CModels where '{Product.CModelsId}' like '%'+cast(id as VARCHAR(50))+'%'");
                        orderItem.CModels = string.Join(",", CModels.Select(p => p.Name));
                    }
                    else
                        orderItem.CModels = "所有";
                    orderItem.CouponETime = Product.CouponETime;
                    orderItem.Validity = Product.CouponValidity;
                    orderItem.Price = Convert.ToDecimal(Product.ProductPrice);
                    orderItem.Amount = orderItem.Price * (decimal)orderItem.Quantity;
                }
                entity.Quantity = entity.OrderItems.Sum(p => p.Quantity);
                entity.Amount = entity.OrderItems.Sum(p => p.Amount);
                entity.Balance = entity.Amount;
                await OrderRepository.InsertAsync(entity);
                await CurrentUnitOfWork.SaveChangesAsync();

                return new { OrderId = entity.Id };
            }
            catch (Exception e)
            {
                Logger.Error("订单保存异常！", e);
                throw new AbpException("订单保存异常！", e);
            }
        }
        #endregion

        #region 修改支付信息
        [AbpAuthorize("Admin.Order.Edit")]
        public async Task AdminOrderSaveAsync(AdminOrderEditDto input)
        {
            if (input.Id == 0)
                throw new AbpException("错误的参数Id");
            var Order = await OrderRepository.FirstOrDefaultAsync(p => p.Id == input.Id);
            if (Order == null)
                throw new AbpException("错误的参数Id");
            Order.State = input.State;
            Order.Remarks = input.Remarks;
            Order.PayCer = input.PayCer;
            Order.PayMode = input.PayMode;
            await OrderRepository.UpdateAsync(Order);
            await CurrentUnitOfWork.SaveChangesAsync();
        }
        #endregion

        #region 新增订单的默认参数
        /// <summary>
        /// 新增订单的默认参数
        /// </summary>
        /// <returns></returns>
        public async Task<OrderEditDto> GetEmptyEntityAsync()
        {
            return await Task.Run(() => new OrderEditDto());
        }
        #endregion

        #region 批量删除订单
        /// <summary>
        /// 批量删除订单
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete]
        [AbpAuthorize("Admin.Order.Edit")]
        public async Task BatchDeleteAsync(string ids)
        {
            try
            {
                foreach (var id in ids.Split(','))
                {
                    await OrderRepository.DeleteAsync(kv => kv.Id == int.Parse(id));
                }
                await CurrentUnitOfWork.SaveChangesAsync();

            }
            catch (Exception e)
            {
                Logger.Error("订单删除异常！", e);
                throw new AbpException("订单删除异常！", e);
            }
        }
        #endregion

        #region 核销
        /// <summary>
        /// 核销
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AbpAuthorize("Admin.Order.WriteOff")]
        public async Task WriteOff(string ids)
        {
            try
            {
                foreach (var id in ids.Split(','))
                {
                    var entity = await OrderRepository.GetAll().Include(p => p.OrderItems).FirstOrDefaultAsync(p => p.Id == int.Parse(id));
                    OrderUseAggregate orderUse = new OrderUseAggregate();
                    orderUse.OrderId = entity.Id;
                    orderUse.GoodsName = string.Join(",", entity.OrderItems.Select(p => p.ProductName));
                    orderUse.ETime = entity.OrderItems.FirstOrDefault().CouponETime;
                    //orderUse.
                    await OrderRepository.UpdateAsync(entity);
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Logger.Error("订单核销异常！", e);
                throw new AbpException("订单核销异常！", e);
            }
        }
        #endregion

        #region 获取统计
        /// <summary>
        /// 获取统计
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<object> Total(int id)
        {

            var list = OrderRepository.GetAll().AsEnumerable();
            if (id != 0)
                list = list.Where(p => p.IsDeleted == false && p.DealerId == id);
            return new
            {
                n = "订单",
                n1 = "总订单数",
                n2 = "总销售额",
                n3 = "订单取消数",
                t1 = list.Count(),
                t2 = list.Sum(p => p.Amount),
                t3 = list.Where(p => p.Progress == 3).Count()
            };
        }
        #endregion
    }

    public class OrderEditDto
    {

        /// <summary>
        /// 经销商Id
        /// </summary>
        public int DealerId { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// 订单详细
        /// </summary>
        public List<OrderItemEditDto> OrderItems { get; set; }

        public OrderEditDto()
        {
            OrderItems = new List<OrderItemEditDto>();
        }

    }

    public class AdminOrderEditDto : EntityDto
    {
        public int State { get; set; }

        public string Remarks { get; set; }
        /// <summary>
        /// 支付方式
        /// </summary>
        public int PayMode { get; set; }

        /// <summary>
        /// 支付凭证
        /// </summary>
        public string PayCer { get; set; }
    }
    public class OrderDto : EntityDto
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

        /// <summary>
        /// 总金额
        /// </summary>
        public decimal Amount { get; set; }


        /// <summary>
        /// 单价
        /// </summary>
        public decimal Price
        {
            get
            {
                var item = this.OrderItems.Sum(p => p.Price);
                return item;
            }
        }

        /// <summary>
        /// 总数量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 状态 0待付款 1已付款 2已退款
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
                    return "已付款";
                else if (this.State == 2)
                    return "已退款";
                else
                    return "待付款";
            }
        }

        /// <summary>
        /// 状态 0未使用/1使用中/2已使用/3失效
        /// </summary>
        public int Progress { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string ProgressText
        {
            get
            {
                if (this.Progress == 1)
                    return "使用中";
                else if (this.Progress == 2)
                    return "已使用";
                else if (this.Progress == 3)
                    return "失效";
                else
                    return "未使用";
            }
        }
        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; }

        public DateTime CreationTime { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName
        {
            get
            {
                var item = string.Join(",", this.OrderItems.Select(p => p.ProductName));
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

        /// <summary>
        /// 订单详细
        /// </summary>
        public List<OrderItemDto> OrderItems { get; set; }
        public string StorePhone { get; set; }
        public string StoreAddress { get; set; }

        /// <summary>
        /// 支付方式
        /// </summary>
        public int PayMode { get; set; }

        /// <summary>
        /// 支付凭证
        /// </summary>
        public string PayCer { get; set; }
        public decimal Balance { get; set; }

        /// <summary>
        /// 支付时间
        /// </summary>
        public DateTime PayTime { get; set; }
    }

    public class OrderItemEditDto
    {
        /// <summary>
        /// 产品Id
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }

    }

    public class OrderItemDto
    {
        /// <summary>
        /// 产品Id
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 车型Id
        /// </summary>
        public string CModelIds { get; set; }

        /// <summary>
        /// 车型
        /// </summary>
        public string CModels { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 单价
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 总额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 产品图片
        /// </summary>
        public string ProductPic { get; set; }

        /// <summary>
        /// 卡劵有效期
        /// </summary>
        public DateTime CouponETime { get; set; }

        /// <summary>
        /// 有效期
        /// </summary>
        public string Validity { get; set; }
    }
}
