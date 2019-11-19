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

namespace IF.Porsche
{
    public class ShoppingCartAppService : IFAppServiceBase, IShoppingCartAppService
    {
        IRepository<ShoppingCartAggregate> ShoppingCartRepository;
        MSSqlHelper sqlHelper;
        public ShoppingCartAppService(
            IRepository<ShoppingCartAggregate> ShoppingCartRepository)
        {
            this.ShoppingCartRepository = ShoppingCartRepository;
            sqlHelper = new MSSqlHelper(AppConfigurations.GetAppSettings().GetSection("ConnectionStrings").GetSection("OutSide").Value);
        }
        /// <summary>
        /// ��ȡ���ﳵ�������Ϣ��֧�ַ�ҳ��ѯ
        /// </summary>
        /// <param name="loadOptions">��ѯ����</param>
        /// <returns></returns>
        public async Task<object> GetListDataAsync(DataSourceLoadOptions loadOptions)
        {
            var list = sqlHelper.Query<GoodsDto>(@"select ProductId,ProductName,ProductPic,ProductDetailPic,ProductSales,ProductPrice,DisplayPrice,ProductDesc,DealerId from Products");
            var result = from ShoppingCart in ShoppingCartRepository.GetAll().AsEnumerable()
                         join Product in list
                         on ShoppingCart.ProductId equals Product.ProductId
                         select new ShoppingCartDto
                         {
                             Id = ShoppingCart.Id,
                             UserId = ShoppingCart.UserId,
                             DealerId = ShoppingCart.DealerId,
                             ProductId = ShoppingCart.ProductId,
                             Quantity = ShoppingCart.Quantity,
                             ProductName = Product.ProductName,
                             ProductPic = Product.ProductPic,
                             ProductPrice = Product.ProductPrice,
                             ProductDesc = Product.ProductDesc,
                             ProductDealerId = Product.DealerId
                         };
            return base.DataSourceLoadMap(result, loadOptions);
        }

        /// <summary>
        /// ��ȡ���ﳵ�������Ϣ��֧�ַ�ҳ��ѯ
        /// </summary>
        /// <param name="loadOptions">��ѯ����</param>
        /// <returns></returns>
        public async Task<object> GetMyListDataAsync(DataSourceLoadOptions loadOptions)
        {
            var list = sqlHelper.Query<GoodsDto>(@"select ProductId,ProductName,ProductPic,ProductDetailPic,ProductSales,ProductPrice,DisplayPrice,ProductDesc,DealerId from Products");
            var result = from ShoppingCart in ShoppingCartRepository.GetAll().Where(p => p.UserId == AbpSession.UserId.Value).AsEnumerable()
                         join Product in list
                         on ShoppingCart.ProductId equals Product.ProductId
                         select new ShoppingCartDto
                         {
                             Id = ShoppingCart.Id,
                             UserId = ShoppingCart.UserId,
                             DealerId = ShoppingCart.DealerId,
                             ProductId = ShoppingCart.ProductId,
                             Quantity = ShoppingCart.Quantity,
                             ProductName = Product.ProductName,
                             ProductPic = Product.ProductPic,
                             ProductPrice = Product.ProductPrice,
                             ProductDesc = Product.ProductDesc,
                             ProductDealerId = Product.DealerId
                         };
            return base.DataSourceLoadMap(result, loadOptions);
        }

        #region ����ID��ȡ���ﳵ��Ϣ
        /// <summary>
        /// ����ID��ȡ���ﳵ��Ϣ
        /// </summary>
        /// <param name="id">���ﳵ��Ϣ����</param>
        /// <returns></returns>
        public async Task<ShoppingCartDto> GetAsync(int id)
        {
            try
            {
                var ShoppingCart = await ShoppingCartRepository.GetAll().Where(k => k.Id == id).FirstOrDefaultAsync();

                if (ShoppingCart == null)
                {
                    throw new EntityNotFoundException(typeof(ShoppingCartAggregate), id);
                }
                var dto = this.ObjectMapper.Map<ShoppingCartDto>(ShoppingCart);
                return dto;
            }
            catch (Exception e)
            {
                Logger.Error("���ﳵ��ȡ�쳣��", e);
                throw new AbpException("���ﳵ��ȡ�쳣", e);
            }
        }
        #endregion

        #region  �������߸��Ĺ��ﳵ��Ϣ
        /// <summary>
        /// �������߸��Ĺ��ﳵ��Ϣ
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task SaveAsync(ShoppingCartEditDto input)
        {
            try
            {
                ShoppingCartAggregate entity = this.ObjectMapper.Map<ShoppingCartAggregate>(input);
                entity.UserId = AbpSession.UserId.Value;
                ShoppingCartAggregate p = ShoppingCartRepository.GetAll().Where(k =>k.DealerId== input.DealerId && k.ProductId == input.ProductId && k.UserId == AbpSession.UserId.Value).FirstOrDefault();
                if (input.Id != 0)
                {
                    ShoppingCartAggregate ShoppingCartAggregate = ShoppingCartRepository.GetAll().Where(k => k.Id == input.Id).FirstOrDefault();
                    ShoppingCartAggregate.DealerId = entity.DealerId;
                    ShoppingCartAggregate.ProductId = entity.ProductId;
                    ShoppingCartAggregate.Quantity = entity.Quantity;

                    await ShoppingCartRepository.UpdateAsync(ShoppingCartAggregate);
                }
                else  if (p != null)
                {
                    p.Quantity += entity.Quantity;
                    await ShoppingCartRepository.UpdateAsync(p);
                }
                else
                {
                    await ShoppingCartRepository.InsertAsync(entity);
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            catch (Exception e)
            {
                Logger.Error("���ﳵ�����쳣��", e);
                throw new AbpException("���ﳵ�����쳣��", e);
            }
        }
        #endregion

        #region �������ﳵ��Ĭ�ϲ���
        /// <summary>
        /// �������ﳵ��Ĭ�ϲ���
        /// </summary>
        /// <returns></returns>
        public async Task<ShoppingCartEditDto> GetEmptyEntityAsync()
        {
            return await Task.Run(() => new ShoppingCartEditDto());
        }
        #endregion

        #region ����ɾ�����ﳵ
        /// <summary>
        /// ����ɾ�����ﳵ
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete]
        public async Task BatchDeleteAsync(string ids)
        {
            try
            {
                foreach (var id in ids.Split(','))
                {
                    await ShoppingCartRepository.DeleteAsync(kv => kv.UserId == AbpSession.UserId.Value && kv.Id == int.Parse(id));
                }
                await CurrentUnitOfWork.SaveChangesAsync();

            }
            catch (Exception e)
            {
                Logger.Error("���ﳵɾ���쳣��", e);
                throw new AbpException("���ﳵɾ���쳣��", e);
            }
        }
        #endregion
    }

    public class ShoppingCartEditDto : EntityDto
    {
        /// <summary>
        /// ������Id
        /// </summary>
        public int DealerId { get; set; }
        /// <summary>
        /// ��ƷId
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public int Quantity { get; set; }
    }

    public class ShoppingCartDto : EntityDto
    {
        /// <summary>
        /// �û�Id
        /// </summary>
        public long UserId { get; set; }
        /// <summary>
        /// ������Id
        /// </summary>
        public int DealerId { get; set; }

        /// <summary>
        /// ��ƷId
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>
        /// ��Ʒ����
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// ��Ʒ�۸�
        /// </summary>
        public double ProductPrice { get; set; }

        /// <summary>
        /// ��ƷͼƬ
        /// </summary>
        public string ProductPic { get; set; }

        /// <summary>
        /// ��Ʒ����
        /// </summary>
        public string ProductDesc { get; set; }
        /// <summary>
        /// ���þ�����
        /// </summary>
        public string ProductDealerId { get; set; }

    }
}
