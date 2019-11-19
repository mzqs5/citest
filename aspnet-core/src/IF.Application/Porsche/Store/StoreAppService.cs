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

namespace IF.Porsche
{
    public class StoreAppService : IFAppServiceBase, IStoreAppService
    {
        IRepository<StoreAggregate> StoreRepository;
        public StoreAppService(
            IRepository<StoreAggregate> StoreRepository)
        {
            this.StoreRepository = StoreRepository;
        }
        /// <summary>
        /// 获取经销商的相关信息，支持分页查询
        /// </summary>
        /// <param name="loadOptions">查询参数</param>
        /// <returns></returns>
        public async Task<object> GetListDataAsync(DataSourceLoadOptions loadOptions)
        {
            var result = from Store in StoreRepository.GetAll().AsEnumerable()
                         select new StoreDto
                         {
                             Id = Store.Id,
                             Name = Store.Name,
                             Date = Store.Date,
                             Address = Store.Address,
                             Phone = Store.Phone,
                             More = Store.More,
                             EName = Store.EName,
                             EDate = Store.EDate,
                             EAddress = Store.EAddress,
                             EPhone = Store.EPhone,
                             EMore = Store.EMore,
                             Type = Store.Type
                         };
            return base.DataSourceLoadMap(result, loadOptions);
        }

        #region 根据ID获取经销商信息
        /// <summary>
        /// 根据ID获取经销商信息
        /// </summary>
        /// <param name="id">经销商信息主键</param>
        /// <returns></returns>
        public async Task<StoreDto> GetAsync(int id)
        {
            try
            {
                var Store = await StoreRepository.GetAll().Where(k => k.Id == id).FirstOrDefaultAsync();

                if (Store == null)
                {
                    throw new EntityNotFoundException(typeof(StoreAggregate), id);
                }
                var dto = this.ObjectMapper.Map<StoreDto>(Store);
                return dto;
            }
            catch (Exception e)
            {
                Logger.Error("经销商获取异常！", e);
                throw new AbpException("经销商获取异常", e);
            }
        }
        #endregion

        #region  新增或者更改经销商信息
        /// <summary>
        /// 新增或者更改经销商信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [AbpAuthorize("Admin.Store.Edit")]
        public async Task SaveAsync(StoreEditDto input)
        {
            try
            {
                StoreAggregate entity = this.ObjectMapper.Map<StoreAggregate>(input);
                if (input.Id != 0)
                {
                    StoreAggregate StoreAggregate = StoreRepository.GetAll().Where(k => k.Id == input.Id).FirstOrDefault();
                    StoreAggregate.Name = entity.Name;
                    StoreAggregate.Phone = entity.Phone;
                    StoreAggregate.Address = entity.Address;
                    StoreAggregate.Date = entity.Date;
                    StoreAggregate.More = entity.More;
                    StoreAggregate.EName = entity.EName;
                    StoreAggregate.EPhone = entity.EPhone;
                    StoreAggregate.EAddress = entity.EAddress;
                    StoreAggregate.EDate = entity.EDate;
                    StoreAggregate.EMore = entity.EMore;
                    StoreAggregate.Type = entity.Type;
                    await StoreRepository.UpdateAsync(StoreAggregate);
                }
                else
                {
                    await StoreRepository.InsertAsync(entity);
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            catch (Exception e)
            {
                Logger.Error("经销商保存异常！", e);
                throw new AbpException("经销商保存异常！", e);
            }
        }
        #endregion

        #region 新增经销商的默认参数
        /// <summary>
        /// 新增经销商的默认参数
        /// </summary>
        /// <returns></returns>
        public async Task<StoreEditDto> GetEmptyEntityAsync()
        {
            return await Task.Run(() => new StoreEditDto());
        }
        #endregion

        #region 批量删除经销商
        /// <summary>
        /// 批量删除经销商
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
                    await StoreRepository.DeleteAsync(kv => kv.Id == int.Parse(id));
                }
                await CurrentUnitOfWork.SaveChangesAsync();

            }
            catch (Exception e)
            {
                Logger.Error("经销商删除异常！", e);
                throw new AbpException("经销商删除异常！", e);
            }
        }
        #endregion
    }

    public class StoreEditDto : EntityDto
    {
        /// <summary>
        /// 经销商名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 经销商名称 英文
        /// </summary>
        public string EName { get; set; }

        /// <summary>
        /// 经销商电话
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 经销商电话英文
        /// </summary>
        public string EPhone { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 地址英文
        /// </summary>
        public string EAddress { get; set; }

        /// <summary>
        /// 营业时间
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// 营业时间英文
        /// </summary>
        public string EDate { get; set; }

        /// <summary>
        /// 更多
        /// </summary>
        public string More { get; set; }


        /// <summary>
        /// 更多英文
        /// </summary>
        public string EMore { get; set; }

        /// <summary>
        /// 类型 1保时捷，2阿斯顿马丁
        /// </summary>
        public int Type { get; set; }
    }

    public class StoreDto : EntityDto
    {
        /// <summary>
        /// 经销商名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 经销商名称 英文
        /// </summary>
        public string EName { get; set; }

        /// <summary>
        /// 经销商电话
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 经销商电话英文
        /// </summary>
        public string EPhone { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 地址英文
        /// </summary>
        public string EAddress { get; set; }

        /// <summary>
        /// 营业时间
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// 营业时间英文
        /// </summary>
        public string EDate { get; set; }

        /// <summary>
        /// 更多
        /// </summary>
        public string More { get; set; }


        /// <summary>
        /// 更多英文
        /// </summary>
        public string EMore { get; set; }

        /// <summary>
        /// 类型 1保时捷，2阿斯顿马丁
        /// </summary>
        public int Type { get; set; }
    }
}
