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
using Abp.Collections.Extensions;
using IF.Common;
using DbHelper;
using IF.Configuration;

namespace IF.Porsche
{
    /// <summary>
    /// 商品应用服务
    /// </summary>
    public class ProductAppService : IFAppServiceBase, IProductAppService
    {
        MSSqlHelper sqlHelper;
        public ProductAppService(
)
        {
            sqlHelper = new MSSqlHelper(AppConfigurations.GetAppSettings().GetSection("ConnectionStrings").GetSection("OutSide").Value);
        }

        /// <summary>
        /// 获取商品的相关信息，支持分页查询
        /// </summary>
        /// <param name="loadOptions">查询参数</param>
        /// <returns></returns>
        public async Task<object> QueryListDataAsync(QueryDto query)
        {
            List<string> queryW = new List<string>();
            //queryW.Add($"product.ProductId in({AppConfigurations.GetAppSettings().GetSection("FilterProduct").Value})");
            //queryW.Add($"product.DealerId in(select Id from Dealer where Name like '%-PMI')");
            if (query.DealerId.HasValue)
                queryW.Add($"(product.DealerId like '%{query.DealerId.Value}%')");
            if (query.CarId.HasValue)
                queryW.Add($"(coupon.CModelsId like '%{query.CarId.Value}%' or coupon.CModelsId like '%,0,%')");
            var list = sqlHelper.Query<GoodsDto>(string.Format(@"select ProductId,ProductName,ProductPic,ProductDetailPic,ProductSales,ProductPrice,DisplayPrice,ProductDesc,Processing,Mask,CouponId,DealerId,PriceCount,ProductStatus,IsTop,Name as CouponName,ETime as CouponETime,Validity as CouponValidity,[Desc] as CouponDesc,Tags as CouponTags,Money as CouponMoney,Blance as CouponBlance,UseNumber as CouponUseNumber,CModelsId,Status as CouponStatus from Products as product
                            left join Coupon as coupon on product.CouponId = coupon.id
                        {0} order by product.IsTop", queryW.Count > 0 ? " where " + string.Join(" and ", queryW) : ""));

            return new DataSourceLoadResult<GoodsDto>() { data = list.ToList() };
        }

        public async Task<IActionResult> Export(QueryDto query)
        {
            List<string> queryW = new List<string>();
            //queryW.Add($"product.ProductId in({AppConfigurations.GetAppSettings().GetSection("FilterProduct").Value})");
            //queryW.Add($"product.DealerId in(select Id from Dealer where Name like '%-PMI')");
            if (query.DealerId.HasValue)
                queryW.Add($"(product.DealerId like '%{query.DealerId.Value}%')");
            if (query.CarId.HasValue)
                queryW.Add($"(coupon.CModelsId like '%{query.CarId.Value}%' or coupon.CModelsId like '%,0,%')");
            var list = sqlHelper.Query<GoodsDto>(string.Format(@"select ProductId,ProductName,ProductPic,ProductDetailPic,ProductSales,ProductPrice,DisplayPrice,ProductDesc,Processing,Mask,CouponId,DealerId,PriceCount,ProductStatus,IsTop,Name as CouponName,ETime as CouponETime,Validity as CouponValidity,[Desc] as CouponDesc,Tags as CouponTags,Money as CouponMoney,Blance as CouponBlance,UseNumber as CouponUseNumber,CModelsId,Status as CouponStatus from Products as product
                            left join Coupon as coupon on product.CouponId = coupon.id
                        {0} order by product.IsTop", queryW.Count > 0 ? " where " + string.Join(" and ", queryW) : ""));
            var Export = new ExportHelper();
            Export.AddColumn("ProductId", "产品Id");
            Export.AddColumn("ProductName", "产品名称");
            Export.AddColumn("ProductPic", "产品图片");
            var result = Export.ExportExcel<GoodsDto>(list.ToList());
            return result;
        }

        #region 根据ID获取商品信息
        /// <summary>
        /// 根据ID获取商品信息
        /// </summary>
        /// <param name="id">商品信息主键</param>
        /// <returns></returns>
        public async Task<GoodsDto> GetAsync(int id)
        {
            try
            {
                var list = sqlHelper.Query<GoodsDto>(string.Format(@"select ProductId,ProductName,ProductPic,ProductDetailPic,ProductSales,ProductPrice,DisplayPrice,ProductDesc,Processing,Mask,CouponId,DealerId,PriceCount,ProductStatus,IsTop,Name as CouponName,ETime as CouponETime,Validity as CouponValidity,[Desc] as CouponDesc,Tags as CouponTags,Money as CouponMoney,Blance as CouponBlance,UseNumber as CouponUseNumber,CModelsId,Status as CouponStatus from Products as product
                            left join Coupon as coupon on product.CouponId = coupon.id
                        where product.ProductId={0}", id));
                var Goods = list.FirstOrDefault();

                if (Goods == null)
                {
                    throw new EntityNotFoundException(typeof(GoodsAggregate), id);
                }
                //var dto = this.ObjectMapper.Map<GoodsDto>(Goods);
                return Goods;
            }
            catch (Exception e)
            {
                Logger.Error("商品获取异常！", e);
                throw new AbpException("商品获取异常", e);
            }
        }
        #endregion
    }

    public class QueryDto
    {
        /// <summary>
        /// 按车型Id查询
        /// </summary>
        public int? CarId { get; set; }

        /// <summary>
        /// 按经销商Id查询
        /// </summary>
        public int? DealerId { get; set; }
    }

    public class GoodsEditDto : EntityDto
    {

        /// <summary>
        /// 商品名称
        /// </summary>
        [Required]
        public string ProductName { get; set; }

        /// <summary>
        /// 商品图片
        /// </summary>
        public string ProductPic { get; set; }

        /// <summary>
        /// 商品详情图
        /// </summary>
        public string ProductDetailPic { get; set; }


        /// <summary>
        /// 商品售价
        /// </summary>
        [Required]
        public float ProductSales { get; set; }


        /// <summary>
        /// 商品价格
        /// </summary>
        [Required]
        public float ProductPrice { get; set; }

        /// <summary>
        /// 显示价格
        /// </summary>
        public float? DisplayPrice { get; set; }

        /// <summary>
        /// 商品详情
        /// </summary>
        public string ProductDesc { get; set; }

        /// <summary>
        /// 处理
        /// </summary>
        public float Processing { get; set; }

        /// <summary>
        /// 封面
        /// </summary>
        public string Mask { get; set; }

        /// <summary>
        /// 卡券Id
        /// </summary>
        public int CouponId { get; set; }

        /// <summary>
        /// 经销商Id 门店
        /// </summary>
        public string DealerId { get; set; }

        /// <summary>
        /// 价格计算
        /// </summary>
        public float? PriceCount { get; set; }

        /// <summary>
        /// 商品状态
        /// </summary>
        public int ProductStatus { get; set; }

        /// <summary>
        /// 是否置顶
        /// </summary>
        public int? IsTop { get; set; }
    }

    public class GoodsDto
    {
        /// <summary>
        /// 商品Id
        /// </summary>
        public int ProductId { get; set; }
        /// <summary>
        /// 商品名称
        /// </summary>
        public string ProductName { get; set; }
        /// <summary>
        /// 商品图片
        /// </summary>
        public string ProductPic { get; set; }
        /// <summary>
        /// 商品详情图
        /// </summary>
        public string ProductDetailPic { get; set; }

        /// <summary>
        /// 商品售价
        /// </summary>
        public double ProductSales { get; set; }

        /// <summary>
        /// 商品价格
        /// </summary>
        public Double ProductPrice { get; set; }

        /// <summary>
        /// 显示价格
        /// </summary>
        public Double DisplayPrice { get; set; }


        /// <summary>
        /// 商品详情
        /// </summary>
        public string ProductDesc { get; set; }

        /// <summary>
        /// 处理
        /// </summary>
        public Double Processing { get; set; }

        /// <summary>
        /// 封面
        /// </summary>
        public string Mask { get; set; }

        /// <summary>
        /// 卡券Id
        /// </summary>
        public int CouponId { get; set; }

        /// <summary>
        /// 经销商Id 门店
        /// </summary>
        public string DealerId { get; set; }

        /// <summary>
        /// 价格计算
        /// </summary>
        public Double PriceCount { get; set; }

        /// <summary>
        /// 商品状态
        /// </summary>
        public int ProductStatus { get; set; }

        /// <summary>
        /// 是否置顶
        /// </summary>
        public int IsTop { get; set; }

        /// <summary>
        /// 卡劵名称
        /// </summary>
        public string CouponName { get; set; }

        /// <summary>
        /// 有效时间
        /// </summary>
        public DateTime CouponETime { get; set; }

        /// <summary>
        /// 有效性
        /// </summary>
        public string CouponValidity { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [MaxLength(4000)]
        public string CouponDesc { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        public string CouponTags { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public Double CouponMoney { get; set; }

        /// <summary>
        /// 余额
        /// </summary>
        public Double CouponBlance { get; set; }

        /// <summary>
        /// 使用数量
        /// </summary>
        public int CouponUseNumber { get; set; }

        /// <summary>
        /// 车型Id
        /// </summary>
        public string CModelsId { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string CouponStatus { get; set; }
        /// <summary>
        /// 车型名称
        /// </summary>
        //public string CarName { get; set; }
    }
}
