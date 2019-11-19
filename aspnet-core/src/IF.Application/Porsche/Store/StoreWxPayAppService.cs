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
    public class StoreWxPayAppService : IFAppServiceBase, IStoreWxPayAppService
    {
        IRepository<StoreWxPayAggregate> StoreWxPayRepository;
        IRepository<DealerAggregate> DealerRepository;
        MSSqlHelper sqlHelper;
        public StoreWxPayAppService(
            IRepository<StoreWxPayAggregate> StoreWxPayRepository,
            IRepository<DealerAggregate> DealerRepository)
        {
            this.StoreWxPayRepository = StoreWxPayRepository;
            this.DealerRepository = DealerRepository;
            sqlHelper = new MSSqlHelper(AppConfigurations.GetAppSettings().GetSection("ConnectionStrings").GetSection("OutSide").Value);
        }
        /// <summary>
        /// 获取经销商支付商户的相关信息，支持分页查询
        /// </summary>
        /// <param name="loadOptions">查询参数</param>
        /// <returns></returns>
        public async Task<object> GetListDataAsync(DataSourceLoadOptions loadOptions)
        {
            var DealerList = sqlHelper.Query<DealerDto>(
                string.Format(@"select Id,Name,Distance,Address,Phone,Status,Longitude,Latitude,AUserId,DDesc,System_PlaceId from Dealer
                           where Name like '%-PMI'"));
            var result = from StoreWxPay in StoreWxPayRepository.GetAll().AsEnumerable()
                         join Dealer in DealerList
                         on StoreWxPay.StoreId equals Dealer.Id
                         select new StoreWxPayDto
                         {
                             Id = StoreWxPay.Id,
                             StoreId = StoreWxPay.StoreId,
                             StoreName = Dealer.Name.Replace("-PMI", ""),
                             appid = StoreWxPay.appid,
                             secret = StoreWxPay.secret,
                             mch_id = StoreWxPay.mch_id,
                             key = StoreWxPay.key
                         };
            return base.DataSourceLoadMap(result, loadOptions);
        }

        #region 根据ID获取经销商支付商户信息
        /// <summary>
        /// 根据ID获取经销商支付商户信息
        /// </summary>
        /// <param name="id">经销商支付商户信息主键</param>
        /// <returns></returns>
        public async Task<StoreWxPayDto> GetAsync(int id)
        {
            try
            {
                var StoreWxPay = await StoreWxPayRepository.GetAll().Where(k => k.Id == id).FirstOrDefaultAsync();

                if (StoreWxPay == null)
                {
                    throw new EntityNotFoundException(typeof(StoreWxPayAggregate), id);
                }
                var dto = this.ObjectMapper.Map<StoreWxPayDto>(StoreWxPay);
                return dto;
            }
            catch (Exception e)
            {
                Logger.Error("经销商支付商户获取异常！", e);
                throw new AbpException("经销商支付商户获取异常", e);
            }
        }
        #endregion

        #region  新增或者更改经销商支付商户信息
        /// <summary>
        /// 新增或者更改经销商支付商户信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [AbpAuthorize("Admin.Store.Edit")]
        public async Task SaveAsync(StoreWxPayEditDto input)
        {
            try
            {
                StoreWxPayAggregate entity = this.ObjectMapper.Map<StoreWxPayAggregate>(input);
                if (input.Id != 0)
                {
                    StoreWxPayAggregate StoreWxPayAggregate = StoreWxPayRepository.GetAll().Where(k => k.Id == input.Id).FirstOrDefault();
                    StoreWxPayAggregate.appid = entity.appid;
                    StoreWxPayAggregate.secret = entity.secret;
                    StoreWxPayAggregate.mch_id = entity.mch_id;
                    StoreWxPayAggregate.key = entity.key;

                    await StoreWxPayRepository.UpdateAsync(StoreWxPayAggregate);
                }
                else
                {
                    await StoreWxPayRepository.InsertAsync(entity);
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            catch (Exception e)
            {
                Logger.Error("经销商支付商户保存异常！", e);
                throw new AbpException("经销商支付商户保存异常！", e);
            }
        }
        #endregion

        #region 新增经销商支付商户的默认参数
        /// <summary>
        /// 新增经销商支付商户的默认参数
        /// </summary>
        /// <returns></returns>
        public async Task<StoreWxPayEditDto> GetEmptyEntityAsync()
        {
            return await Task.Run(() => new StoreWxPayEditDto());
        }
        #endregion

        #region 批量删除经销商支付商户
        /// <summary>
        /// 批量删除经销商支付商户
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete]
        [AbpAuthorize("Admin.Store.Edit")]
        public async Task BatchDeleteAsync(string ids)
        {
            try
            {
                foreach (var id in ids.Split(','))
                {
                    await StoreWxPayRepository.DeleteAsync(kv => kv.Id == int.Parse(id));
                }
                await CurrentUnitOfWork.SaveChangesAsync();

            }
            catch (Exception e)
            {
                Logger.Error("经销商支付商户删除异常！", e);
                throw new AbpException("经销商支付商户删除异常！", e);
            }
        }
        #endregion
    }

    public class StoreWxPayEditDto : EntityDto
    {
        /// <summary>
        /// 经销商Id
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// 公众账号ID
        /// </summary>
        public string appid { get; set; }

        /// <summary>
        /// 应用密钥
        /// </summary>
        public string secret { get; set; }

        /// <summary>
        /// 商户号
        /// </summary>
        public string mch_id { get; set; }

        /// <summary>
        /// 微信支付API密钥
        /// </summary>
        public string key { get; set; }
    }

    public class StoreWxPayDto : EntityDto
    {
        /// <summary>
        /// 经销商Id
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// 经销商
        /// </summary>
        public string StoreName { get; set; }

        /// <summary>
        /// 公众账号ID
        /// </summary>
        public string appid { get; set; }

        /// <summary>
        /// 应用密钥
        /// </summary>
        public string secret { get; set; }

        /// <summary>
        /// 商户号
        /// </summary>
        public string mch_id { get; set; }

        /// <summary>
        /// 微信支付API密钥
        /// </summary>
        public string key { get; set; }
    }
}
