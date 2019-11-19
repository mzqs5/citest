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
        /// 获取购物车的相关信息，支持分页查询
        /// </summary>
        /// <param name="loadOptions">查询参数</param>
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
        /// 获取购物车的相关信息，支持分页查询
        /// </summary>
        /// <param name="loadOptions">查询参数</param>
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

        #region 根据ID获取购物车信息
        /// <summary>
        /// 根据ID获取购物车信息
        /// </summary>
        /// <param name="id">购物车信息主键</param>
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
                Logger.Error("购物车获取异常！", e);
                throw new AbpException("购物车获取异常", e);
            }
        }
        #endregion

        #region  新增或者更改购物车信息
        /// <summary>
        /// 新增或者更改购物车信息
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
                Logger.Error("购物车保存异常！", e);
                throw new AbpException("购物车保存异常！", e);
            }
        }
        #endregion

        #region 新增购物车的默认参数
        /// <summary>
        /// 新增购物车的默认参数
        /// </summary>
        /// <returns></returns>
        public async Task<ShoppingCartEditDto> GetEmptyEntityAsync()
        {
            return await Task.Run(() => new ShoppingCartEditDto());
        }
        #endregion

        #region 批量删除购物车
        /// <summary>
        /// 批量删除购物车
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
                Logger.Error("购物车删除异常！", e);
                throw new AbpException("购物车删除异常！", e);
            }
        }
        #endregion
    }

    public class ShoppingCartEditDto : EntityDto
    {
        /// <summary>
        /// 经销商Id
        /// </summary>
        public int DealerId { get; set; }
        /// <summary>
        /// 商品Id
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }
    }

    public class ShoppingCartDto : EntityDto
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public long UserId { get; set; }
        /// <summary>
        /// 经销商Id
        /// </summary>
        public int DealerId { get; set; }

        /// <summary>
        /// 商品Id
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 产品价格
        /// </summary>
        public double ProductPrice { get; set; }

        /// <summary>
        /// 产品图片
        /// </summary>
        public string ProductPic { get; set; }

        /// <summary>
        /// 产品描述
        /// </summary>
        public string ProductDesc { get; set; }
        /// <summary>
        /// 可用经销商
        /// </summary>
        public string ProductDealerId { get; set; }

    }
}
