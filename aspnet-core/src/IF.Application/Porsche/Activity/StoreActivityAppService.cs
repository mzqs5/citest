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

namespace IF.Porsche
{
    public class StoreActivityAppService : IFAppServiceBase, IStoreActivityAppService
    {
        IRepository<StoreActivityAggregate> StoreActivityRepository;
        IRepository<StoreAggregate> StoreRepository;
        public StoreActivityAppService(
            IRepository<StoreActivityAggregate> StoreActivityRepository,
            IRepository<StoreAggregate> StoreRepository)
        {
            this.StoreActivityRepository = StoreActivityRepository;
            this.StoreRepository = StoreRepository;
        }
        /// <summary>
        ///  获取活动的相关信息，支持分页查询
        /// </summary>
        /// <param name="loadOptions">查询参数</param>
        /// <returns></returns>
        public async Task<object> GetListDataAsync(DataSourceLoadOptions loadOptions)
        {
            var result = from StoreActivity in StoreActivityRepository.GetAll().AsEnumerable()
                         join Store in StoreRepository.GetAll().AsEnumerable()
                         on StoreActivity.StoreId equals Store.Id
                         select new StoreActivityDto
                         {
                             Id = StoreActivity.Id,
                             Title = StoreActivity.Title,
                             Desc = StoreActivity.Desc,
                             ImgUrl = StoreActivity.ImgUrl,
                             StoreId = StoreActivity.StoreId,
                             StoreName = Store.Name,
                             CreationTime = StoreActivity.CreationTime
                         };
            return base.DataSourceLoadMap(result, loadOptions);
        }

        #region 根据ID获取活动信息
        /// <summary>
        /// 根据ID获取活动信息
        /// </summary>
        /// <param name="id">活动信息主键</param>
        /// <returns></returns>
        public async Task<StoreActivityDto> GetAsync(int id)
        {
            try
            {
                var StoreActivity = await StoreActivityRepository.GetAll().Where(k => k.Id == id).FirstOrDefaultAsync();

                if (StoreActivity == null)
                {
                    throw new EntityNotFoundException(typeof(StoreActivityAggregate), id);
                }
                var dto = this.ObjectMapper.Map<StoreActivityDto>(StoreActivity);
                return dto;
            }
            catch (Exception e)
            {
                Logger.Error("活动获取异常！", e);
                throw new AbpException("活动获取异常", e);
            }
        }
        #endregion

        #region  新增或者更改活动信息
        /// <summary>
        /// 新增或者更改活动信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [AbpAuthorize("Admin.StoreActivity.Edit")]
        public async Task SaveAsync(StoreActivityEditDto input)
        {
            try
            {
                StoreActivityAggregate entity = this.ObjectMapper.Map<StoreActivityAggregate>(input);
                if (input.Id != 0)
                {
                    StoreActivityAggregate StoreActivityAggregate = StoreActivityRepository.GetAll().Where(k => k.Id == input.Id).FirstOrDefault();
                    StoreActivityAggregate.Title = entity.Title;
                    StoreActivityAggregate.Desc = entity.Desc;
                    StoreActivityAggregate.ImgUrl = entity.ImgUrl;
                    StoreActivityAggregate.Detail = entity.Detail;
                    StoreActivityAggregate.StoreId = entity.StoreId;

                    await StoreActivityRepository.UpdateAsync(StoreActivityAggregate);
                }
                else
                {
                    await StoreActivityRepository.InsertAsync(entity);
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            catch (Exception e)
            {
                Logger.Error("活动保存异常！", e);
                throw new AbpException("活动保存异常！", e);
            }
        }
        #endregion

        #region 新增活动的默认参数
        /// <summary>
        /// 新增活动的默认参数
        /// </summary>
        /// <returns></returns>
        public async Task<StoreActivityEditDto> GetEmptyEntityAsync()
        {
            return await Task.Run(() => new StoreActivityEditDto());
        }
        #endregion

        #region 批量删除活动
        /// <summary>
        /// 批量删除活动
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete]
        [AbpAuthorize("Admin.StoreActivity.Edit")]
        public async Task BatchDeleteAsync(string ids)
        {
            try
            {
                foreach (var id in ids.Split(','))
                {
                    await StoreActivityRepository.DeleteAsync(kv => kv.Id == int.Parse(id));
                }
                await CurrentUnitOfWork.SaveChangesAsync();

            }
            catch (Exception e)
            {
                Logger.Error("活动删除异常！", e);
                throw new AbpException("活动删除异常！", e);
            }
        }
        #endregion
    }

    public class StoreActivityEditDto : EntityDto
    {

        /// <summary>
        /// 标题
        /// </summary>
        [Required]
        public string Title { get; set; }

        /// <summary>
        /// 所属门店Id
        /// </summary>
        [Required]
        public int StoreId { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [StringLength(500)]
        public string Desc { get; set; }

        /// <summary>
        /// 封面图
        /// </summary>
        public string ImgUrl { get; set; }


        /// <summary>
        /// 正文
        /// </summary>
        public string Detail { get; set; }
    }

    public class StoreActivityDto : EntityDto
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 所属门店Id
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// 门店名称
        /// </summary>
        public string StoreName { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        /// 封面图
        /// </summary>
        public string ImgUrl { get; set; }

        /// <summary>
        /// 正文
        /// </summary>
        public string Detail { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }
    }
}
