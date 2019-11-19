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
    public class DealerAppService : IFAppServiceBase, IDealerAppService
    {
        IRepository<DealerAggregate> DealerRepository;
        MSSqlHelper sqlHelper;
        public DealerAppService(
            IRepository<DealerAggregate> DealerRepository)
        {
            this.DealerRepository = DealerRepository;
            sqlHelper = new MSSqlHelper(AppConfigurations.GetAppSettings().GetSection("ConnectionStrings").GetSection("OutSide").Value);
        }
        /// <summary>
        /// 获取经销商的相关信息，支持分页查询
        /// </summary>
        /// <param name="loadOptions">查询参数</param>
        /// <returns></returns>
        //public async Task<object> GetListDataAsync(DataSourceLoadOptions loadOptions)
        //{
        //    var result = from Dealer in DealerRepository.GetAll().AsEnumerable()
        //                 where Dealer.Name.Contains("-PMI")
        //                 select new DealerDto
        //                 {
        //                     Id = Dealer.Id,
        //                     Name = Dealer.Name.Replace("-PMI", ""),
        //                     Distance = Dealer.Distance,
        //                     Address = Dealer.Address,
        //                     Phone = Dealer.Phone,
        //                     Status = Dealer.Status,
        //                     Longitude = Dealer.Longitude,
        //                     Latitude = Dealer.Latitude,
        //                     AUserId = Dealer.AUserId,
        //                     DDesc = Dealer.DDesc,
        //                     System_PlaceId = Dealer.System_PlaceId
        //                 };

        //    return base.DataSourceLoadMap(result, loadOptions);
        //}

        /// <summary>
        /// 获取商品的相关信息，支持分页查询
        /// </summary>
        /// <param name="loadOptions">查询参数</param>
        /// <returns></returns>
        public async Task<object> GetListDataAsync(DataSourceLoadOptions loadOptions)
        {
            var list = sqlHelper.Query<DealerDto>(
                string.Format(@"select Id,REPLACE (Name,'-PMI','') as Name,Distance,Address,Phone,Status,Longitude,Latitude,AUserId,DDesc,System_PlaceId from Dealer
                           where Name like '%-PMI'"));

            return base.DataSourceLoadMap(list, loadOptions);
        }

        #region 根据ID获取经销商信息
        /// <summary>
        /// 根据ID获取经销商信息
        /// </summary>
        /// <param name="id">经销商信息主键</param>
        /// <returns></returns>
        public async Task<DealerDto> GetAsync(int id)
        {
            try
            {
                //var Dealer = await DealerRepository.GetAll().Where(k => k.Id == id).FirstOrDefaultAsync();
                var list = sqlHelper.Query<DealerDto>(
                string.Format(@"select Id,Name,Distance,Address,Phone,Status,Longitude,Latitude,AUserId,DDesc,System_PlaceId from Dealer
                           where Id={0}", id));
                var Dealer = list.FirstOrDefault();
                if (Dealer == null)
                {
                    throw new EntityNotFoundException(typeof(DealerAggregate), id);
                }
                var dto = this.ObjectMapper.Map<DealerDto>(Dealer);
                return dto;
            }
            catch (Exception e)
            {
                Logger.Error("经销商获取异常！", e);
                throw new AbpException("经销商获取异常", e);
            }
        }
        #endregion

    }

    public class DealerEditDto : EntityDto
    {
        /// <summary>
        /// 
        /// </summary>
        public string AUserId { get; set; }
        /// <summary>
        /// 经销商名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 经销商详情
        /// </summary>
        public string DDesc { get; set; }

        /// <summary>
        /// 经销商电话
        /// </summary>
        public string Phone { get; set; }

        public int System_PlaceId { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 经度
        /// </summary>
        public string Longitude { get; set; }

        /// <summary>
        /// 纬度
        /// </summary>
        public string Latitude { get; set; }

        /// <summary>
        /// 折扣
        /// </summary>
        public float Distance { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; }
    }

    public class DealerDto : EntityDto
    {
        /// <summary>
        /// 
        /// </summary>
        public string AUserId { get; set; }
        /// <summary>
        /// 经销商名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 经销商详情
        /// </summary>
        public string DDesc { get; set; }

        /// <summary>
        /// 经销商电话
        /// </summary>
        public string Phone { get; set; }

        public int System_PlaceId { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 经度
        /// </summary>
        public string Longitude { get; set; }

        /// <summary>
        /// 纬度
        /// </summary>
        public string Latitude { get; set; }

        /// <summary>
        /// 折扣
        /// </summary>
        public double Distance { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; }
    }
}
