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
    /// ��ƷӦ�÷���
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
        /// ��ȡ��Ʒ�������Ϣ��֧�ַ�ҳ��ѯ
        /// </summary>
        /// <param name="loadOptions">��ѯ����</param>
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
            Export.AddColumn("ProductId", "��ƷId");
            Export.AddColumn("ProductName", "��Ʒ����");
            Export.AddColumn("ProductPic", "��ƷͼƬ");
            var result = Export.ExportExcel<GoodsDto>(list.ToList());
            return result;
        }

        #region ����ID��ȡ��Ʒ��Ϣ
        /// <summary>
        /// ����ID��ȡ��Ʒ��Ϣ
        /// </summary>
        /// <param name="id">��Ʒ��Ϣ����</param>
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
                Logger.Error("��Ʒ��ȡ�쳣��", e);
                throw new AbpException("��Ʒ��ȡ�쳣", e);
            }
        }
        #endregion
    }

    public class QueryDto
    {
        /// <summary>
        /// ������Id��ѯ
        /// </summary>
        public int? CarId { get; set; }

        /// <summary>
        /// ��������Id��ѯ
        /// </summary>
        public int? DealerId { get; set; }
    }

    public class GoodsEditDto : EntityDto
    {

        /// <summary>
        /// ��Ʒ����
        /// </summary>
        [Required]
        public string ProductName { get; set; }

        /// <summary>
        /// ��ƷͼƬ
        /// </summary>
        public string ProductPic { get; set; }

        /// <summary>
        /// ��Ʒ����ͼ
        /// </summary>
        public string ProductDetailPic { get; set; }


        /// <summary>
        /// ��Ʒ�ۼ�
        /// </summary>
        [Required]
        public float ProductSales { get; set; }


        /// <summary>
        /// ��Ʒ�۸�
        /// </summary>
        [Required]
        public float ProductPrice { get; set; }

        /// <summary>
        /// ��ʾ�۸�
        /// </summary>
        public float? DisplayPrice { get; set; }

        /// <summary>
        /// ��Ʒ����
        /// </summary>
        public string ProductDesc { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public float Processing { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string Mask { get; set; }

        /// <summary>
        /// ��ȯId
        /// </summary>
        public int CouponId { get; set; }

        /// <summary>
        /// ������Id �ŵ�
        /// </summary>
        public string DealerId { get; set; }

        /// <summary>
        /// �۸����
        /// </summary>
        public float? PriceCount { get; set; }

        /// <summary>
        /// ��Ʒ״̬
        /// </summary>
        public int ProductStatus { get; set; }

        /// <summary>
        /// �Ƿ��ö�
        /// </summary>
        public int? IsTop { get; set; }
    }

    public class GoodsDto
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
        /// ��ƷͼƬ
        /// </summary>
        public string ProductPic { get; set; }
        /// <summary>
        /// ��Ʒ����ͼ
        /// </summary>
        public string ProductDetailPic { get; set; }

        /// <summary>
        /// ��Ʒ�ۼ�
        /// </summary>
        public double ProductSales { get; set; }

        /// <summary>
        /// ��Ʒ�۸�
        /// </summary>
        public Double ProductPrice { get; set; }

        /// <summary>
        /// ��ʾ�۸�
        /// </summary>
        public Double DisplayPrice { get; set; }


        /// <summary>
        /// ��Ʒ����
        /// </summary>
        public string ProductDesc { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public Double Processing { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string Mask { get; set; }

        /// <summary>
        /// ��ȯId
        /// </summary>
        public int CouponId { get; set; }

        /// <summary>
        /// ������Id �ŵ�
        /// </summary>
        public string DealerId { get; set; }

        /// <summary>
        /// �۸����
        /// </summary>
        public Double PriceCount { get; set; }

        /// <summary>
        /// ��Ʒ״̬
        /// </summary>
        public int ProductStatus { get; set; }

        /// <summary>
        /// �Ƿ��ö�
        /// </summary>
        public int IsTop { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        public string CouponName { get; set; }

        /// <summary>
        /// ��Чʱ��
        /// </summary>
        public DateTime CouponETime { get; set; }

        /// <summary>
        /// ��Ч��
        /// </summary>
        public string CouponValidity { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        [MaxLength(4000)]
        public string CouponDesc { get; set; }

        /// <summary>
        /// ��ǩ
        /// </summary>
        public string CouponTags { get; set; }

        /// <summary>
        /// ���
        /// </summary>
        public Double CouponMoney { get; set; }

        /// <summary>
        /// ���
        /// </summary>
        public Double CouponBlance { get; set; }

        /// <summary>
        /// ʹ������
        /// </summary>
        public int CouponUseNumber { get; set; }

        /// <summary>
        /// ����Id
        /// </summary>
        public string CModelsId { get; set; }

        /// <summary>
        /// ״̬
        /// </summary>
        public string CouponStatus { get; set; }
        /// <summary>
        /// ��������
        /// </summary>
        //public string CarName { get; set; }
    }
}
