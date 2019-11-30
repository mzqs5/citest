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
    /// ����Ӧ�÷���
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

        #region ��ȡ�����������Ϣ��֧�ַ�ҳ��ѯ
        /// <summary>
        /// ��ȡ�����������Ϣ��֧�ַ�ҳ��ѯ����̨ʹ�ã�
        /// </summary>
        /// <param name="loadOptions">��ѯ����</param>
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

        #region ��ȡ�ҵĶ����������Ϣ��֧�ַ�ҳ��ѯ
        /// <summary>
        /// ��ȡ�ҵĶ����������Ϣ��֧�ַ�ҳ��ѯ��ǰ̨ʹ�ã�
        /// </summary>
        /// <param name="loadOptions">��ѯ����</param>
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

        #region ����
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
                Export.AddColumn("OrderNo", "�������");
                Export.AddColumn("StoreName", "�ŵ�����");
                Export.AddColumn("FullName", "����");
                Export.AddColumn("Mobile", "�ֻ�����");
                Export.AddColumn("Amount", "���");
                Export.AddColumn("Quantity", "����");
                Export.AddColumn("Remarks", "��ע");
                Export.AddColumn("CreationTime", "�µ�ʱ��");
                var result1 = Export.ExportExcel<OrderDto>(base.DataSourceLoadMap(result, loadOptions).data);
                result1.FileDownloadName = "������¼";
                return result1;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #endregion

        #region ����ID��ȡ������Ϣ
        /// <summary>
        /// ����ID��ȡ������Ϣ
        /// </summary>
        /// <param name="id">������Ϣ����</param>
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
                Logger.Error("������ȡ�쳣��", e);
                throw new AbpException("������ȡ�쳣", e);
            }
        }
        #endregion

        #region  ����������Ϣ
        /// <summary>
        /// ����������Ϣ
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<object> SaveAsync(OrderEditDto input)
        {
            try
            {
                if (!AbpSession.UserId.HasValue)
                    throw new AbpException("���ȵ�¼��");
                if (input.DealerId == 0)
                    throw new AbpException("��ѡ�����̣�");
                if (input.OrderItems.Count == 0)
                    throw new AbpException("ȱ�ٶ���������Ʒ��ϸ��");
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
                        orderItem.CModels = "����";
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
                Logger.Error("���������쳣��", e);
                throw new AbpException("���������쳣��", e);
            }
        }
        #endregion

        #region �޸�֧����Ϣ
        [AbpAuthorize("Admin.Order.Edit")]
        public async Task AdminOrderSaveAsync(AdminOrderEditDto input)
        {
            if (input.Id == 0)
                throw new AbpException("����Ĳ���Id");
            var Order = await OrderRepository.FirstOrDefaultAsync(p => p.Id == input.Id);
            if (Order == null)
                throw new AbpException("����Ĳ���Id");
            Order.State = input.State;
            Order.Remarks = input.Remarks;
            Order.PayCer = input.PayCer;
            Order.PayMode = input.PayMode;
            await OrderRepository.UpdateAsync(Order);
            await CurrentUnitOfWork.SaveChangesAsync();
        }
        #endregion

        #region ����������Ĭ�ϲ���
        /// <summary>
        /// ����������Ĭ�ϲ���
        /// </summary>
        /// <returns></returns>
        public async Task<OrderEditDto> GetEmptyEntityAsync()
        {
            return await Task.Run(() => new OrderEditDto());
        }
        #endregion

        #region ����ɾ������
        /// <summary>
        /// ����ɾ������
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
                Logger.Error("����ɾ���쳣��", e);
                throw new AbpException("����ɾ���쳣��", e);
            }
        }
        #endregion

        #region ����
        /// <summary>
        /// ����
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
                Logger.Error("���������쳣��", e);
                throw new AbpException("���������쳣��", e);
            }
        }
        #endregion

        #region ��ȡͳ��
        /// <summary>
        /// ��ȡͳ��
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
                n = "����",
                n1 = "�ܶ�����",
                n2 = "�����۶�",
                n3 = "����ȡ����",
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
        /// ������Id
        /// </summary>
        public int DealerId { get; set; }

        /// <summary>
        /// ��ע
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// ������ϸ
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
        /// ֧����ʽ
        /// </summary>
        public int PayMode { get; set; }

        /// <summary>
        /// ֧��ƾ֤
        /// </summary>
        public string PayCer { get; set; }
    }
    public class OrderDto : EntityDto
    {
        /// <summary>
        /// ��ԱId
        /// </summary>
        public long UserId { get; set; }
        /// <summary>
        /// ������Id
        /// </summary>
        public int DealerId { get; set; }

        /// <summary>
        /// ȫ��
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// �ֻ�����
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// �ŵ�����
        /// </summary>
        public string StoreName { get; set; }
        /// <summary>
        /// �������
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// �ܽ��
        /// </summary>
        public decimal Amount { get; set; }


        /// <summary>
        /// ����
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
        /// ������
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// ״̬ 0������ 1�Ѹ��� 2���˿�
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// ״̬
        /// </summary>
        public string StateText
        {
            get
            {
                if (this.State == 1)
                    return "�Ѹ���";
                else if (this.State == 2)
                    return "���˿�";
                else
                    return "������";
            }
        }

        /// <summary>
        /// ״̬ 0δʹ��/1ʹ����/2��ʹ��/3ʧЧ
        /// </summary>
        public int Progress { get; set; }

        /// <summary>
        /// ״̬
        /// </summary>
        public string ProgressText
        {
            get
            {
                if (this.Progress == 1)
                    return "ʹ����";
                else if (this.Progress == 2)
                    return "��ʹ��";
                else if (this.Progress == 3)
                    return "ʧЧ";
                else
                    return "δʹ��";
            }
        }
        /// <summary>
        /// ��ע
        /// </summary>
        public string Remarks { get; set; }

        public DateTime CreationTime { get; set; }

        /// <summary>
        /// ��Ʒ����
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
        /// ����id
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
        /// ����
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
        /// ������ϸ
        /// </summary>
        public List<OrderItemDto> OrderItems { get; set; }
        public string StorePhone { get; set; }
        public string StoreAddress { get; set; }

        /// <summary>
        /// ֧����ʽ
        /// </summary>
        public int PayMode { get; set; }

        /// <summary>
        /// ֧��ƾ֤
        /// </summary>
        public string PayCer { get; set; }
        public decimal Balance { get; set; }

        /// <summary>
        /// ֧��ʱ��
        /// </summary>
        public DateTime PayTime { get; set; }
    }

    public class OrderItemEditDto
    {
        /// <summary>
        /// ��ƷId
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public int Quantity { get; set; }

    }

    public class OrderItemDto
    {
        /// <summary>
        /// ��ƷId
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// ��Ʒ����
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// ����Id
        /// </summary>
        public string CModelIds { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string CModels { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// �ܶ�
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// ��ƷͼƬ
        /// </summary>
        public string ProductPic { get; set; }

        /// <summary>
        /// ������Ч��
        /// </summary>
        public DateTime CouponETime { get; set; }

        /// <summary>
        /// ��Ч��
        /// </summary>
        public string Validity { get; set; }
    }
}
