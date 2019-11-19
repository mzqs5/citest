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
    /// ����ʹ��Ӧ�÷���
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

        #region ��ȡ����ʹ�õ������Ϣ��֧�ַ�ҳ��ѯ
        /// <summary>
        /// ��ȡ����ʹ�õ������Ϣ��֧�ַ�ҳ��ѯ����̨ʹ�ã�
        /// </summary>
        /// <param name="loadOptions">��ѯ����</param>
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

        #region ����ID��ȡ������Ϣ
        /// <summary>
        /// ����ID��ȡ������Ϣ
        /// </summary>
        /// <param name="id">������Ϣ����</param>
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
                Logger.Error("������ȡ�쳣��", e);
                throw new AbpException("������ȡ�쳣", e);
            }
        }
        #endregion

        #region  ��������ʹ����Ϣ
        /// <summary>
        /// ��������ʹ����Ϣ
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [AbpAuthorize("Admin.OrderUse.Edit")]
        public async Task SaveAsync(OrderUseEditDto input)
        {
            if (!AbpSession.UserId.HasValue)
                throw new AbpException("���ȵ�¼��");
            OrderUseAggregate entity = this.ObjectMapper.Map<OrderUseAggregate>(input);

            OrderAggregate Order = OrderRepository.GetAll().Where(k => k.Id == entity.OrderId).FirstOrDefault();
            Order.Progress = 1;
            await OrderRepository.UpdateAsync(Order);
            entity.Balance = Order.Balance;
            await OrderUseRepository.InsertAsync(entity);
            await CurrentUnitOfWork.SaveChangesAsync();
        }
        #endregion

        #region ����
        /// <summary>
        /// ����
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
                    throw new AbpException("���㣡");
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

        #region ����
        /// <summary>
        /// ����
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
        /// ����Id
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// ��Ч��
        /// </summary>
        public DateTime ETime { get; set; }

        /// <summary>
        /// ������Ŀ
        /// </summary>
        public string GoodsName { get; set; }

        /// <summary>
        /// ���ѽ��
        /// </summary>
        public decimal PayAmount { get; set; }

        /// <summary>
        /// ������
        /// </summary>
        public string WorkNo { get; set; }

        /// <summary>
        /// ״̬ 0������/1��ʹ��/2�Ѳ���
        /// </summary>
        //public int State { get; set; }

        /// <summary>
        /// ��ע
        /// </summary>
        public string Remarks { get; set; }

    }

    public class OrderUseDto : EntityDto
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

        public DateTime CreationTime { get; set; }

        /// <summary>
        /// ����Id
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// ��Ч��
        /// </summary>
        public DateTime ETime { get; set; }

        /// <summary>
        /// ������Ŀ
        /// </summary>
        public string GoodsName { get; set; }

        /// <summary>
        /// �����ܽ��
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// ���
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// ���ѽ��
        /// </summary>
        public decimal PayAmount { get; set; }

        /// <summary>
        /// ������
        /// </summary>
        public string WorkNo { get; set; }

        /// <summary>
        /// ״̬ 0������/1��ʹ��/2�Ѳ���
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
                    return "��ʹ��";
                else if (this.State == 2)
                    return "�Ѳ���";
                else
                    return "������";
            }
        }

        /// <summary>
        /// ��ע
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// ������ϸ
        /// </summary>
        public List<OrderItemDto> OrderItems { get; set; }

        /// <summary>
        /// ��Ʒ����
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
    }
}
