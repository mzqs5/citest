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
    public class ActivityAppService : IFAppServiceBase, IActivityAppService
    {
        IRepository<ActivityAggregate> ActivityRepository;
        public ActivityAppService(
            IRepository<ActivityAggregate> ActivityRepository)
        {
            this.ActivityRepository = ActivityRepository;
        }
        /// <summary>
        ///  获取活动的相关信息，支持分页查询
        /// </summary>
        /// <param name="loadOptions">查询参数</param>
        /// <returns></returns>
        public async Task<object> GetListDataAsync(DataSourceLoadOptions loadOptions)
        {
            var result = from Activity in ActivityRepository.GetAll().AsEnumerable()
                         select new ActivityDto
                         {
                             Id = Activity.Id,
                             Title = Activity.Title,
                             Desc = Activity.Desc,
                             Imgs = Activity.Imgs,
                             ImgUrl = Activity.ImgUrl,
                             Link = Activity.Link,
                             Type = Activity.Type,
                             State = Activity.State,
                             Sort = Activity.Sort,
                             Date = Activity.Date,
                             Address = Activity.Address,
                             FromName = Activity.FromName,
                             ETitle = Activity.ETitle,
                             EDesc = Activity.EDesc,
                             EAddress = Activity.EAddress,
                             EFromName = Activity.EFromName,
                             EDate = Activity.EDate,
                             Option = Activity.Option,
                             EOption = Activity.EOption,
                             ThumbnailUrl = Activity.ThumbnailUrl,
                             MobileImgUrl = Activity.MobileImgUrl,
                             MobileThumbnailUrl = Activity.MobileThumbnailUrl,
                             MobileImgs = Activity.MobileImgs
                         };
            return base.DataSourceLoadMap(result, loadOptions);
        }

        #region 根据ID获取活动信息
        /// <summary>
        /// 根据ID获取活动信息
        /// </summary>
        /// <param name="id">活动信息主键</param>
        /// <returns></returns>
        public async Task<ActivityDto> GetAsync(int id)
        {
            try
            {
                var Activity = await ActivityRepository.GetAll().Where(k => k.Id == id).FirstOrDefaultAsync();

                if (Activity == null)
                {
                    throw new EntityNotFoundException(typeof(ActivityAggregate), id);
                }
                var dto = this.ObjectMapper.Map<ActivityDto>(Activity);
                return dto;
            }
            catch (Exception e)
            {
                Logger.Error("活动获取异常！", e);
                throw new AbpException("活动获取异常", e);
            }
        }
        #endregion

        #region 获取最新消息
        /// <summary>
        /// 获取最新消息
        /// </summary>
        /// <returns></returns>
        public async Task<ActivityDto> GetNewAsync()
        {
            try
            {
                var Activity = await ActivityRepository.GetAll().Where(k => k.Type == 2).OrderByDescending(p => p.CreationTime).FirstOrDefaultAsync();

                if (Activity == null)
                {
                    return null;
                }
                var dto = this.ObjectMapper.Map<ActivityDto>(Activity);
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
        [AbpAuthorize("Admin.Activity.Edit")]
        public async Task SaveAsync(ActivityEditDto input)
        {
            try
            {
                ActivityAggregate entity = this.ObjectMapper.Map<ActivityAggregate>(input);
                if (input.Id != 0)
                {
                    ActivityAggregate ActivityAggregate = ActivityRepository.GetAll().Where(k => k.Id == input.Id).FirstOrDefault();
                    ActivityAggregate.Title = entity.Title;
                    ActivityAggregate.PerDetail = entity.PerDetail;
                    ActivityAggregate.BackDetail = entity.BackDetail;
                    ActivityAggregate.Imgs = entity.Imgs;
                    ActivityAggregate.Desc = entity.Desc;
                    ActivityAggregate.ImgUrl = entity.ImgUrl;
                    ActivityAggregate.Link = entity.Link;
                    ActivityAggregate.Type = entity.Type;
                    ActivityAggregate.State = entity.State;
                    ActivityAggregate.Sort = entity.Sort;
                    ActivityAggregate.Date = entity.Date;
                    ActivityAggregate.Address = entity.Address;
                    ActivityAggregate.FromName = entity.FromName;
                    ActivityAggregate.ETitle = entity.ETitle;
                    ActivityAggregate.EPerDetail = entity.EPerDetail;
                    ActivityAggregate.EBackDetail = entity.EBackDetail;
                    ActivityAggregate.EDesc = entity.EDesc;
                    ActivityAggregate.EDate = entity.EDate;
                    ActivityAggregate.EAddress = entity.EAddress;
                    ActivityAggregate.EFromName = entity.EFromName;
                    ActivityAggregate.IntroDetail = entity.IntroDetail;
                    ActivityAggregate.EIntroDetail = entity.EIntroDetail;
                    ActivityAggregate.Option = entity.Option;
                    ActivityAggregate.EOption = entity.EOption;
                    ActivityAggregate.MobileImgUrl = entity.MobileImgUrl;
                    ActivityAggregate.ThumbnailUrl = entity.ThumbnailUrl;
                    ActivityAggregate.MobileThumbnailUrl = entity.MobileThumbnailUrl;
                    ActivityAggregate.MobileImgs = entity.MobileImgs;

                    await ActivityRepository.UpdateAsync(ActivityAggregate);
                }
                else
                {
                    await ActivityRepository.InsertAsync(entity);
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
        public async Task<ActivityEditDto> GetEmptyEntityAsync()
        {
            return await Task.Run(() => new ActivityEditDto());
        }
        #endregion

        #region 批量删除活动
        /// <summary>
        /// 批量删除活动
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete]
        [AbpAuthorize(PermissionNames.Pages_Users)]
        public async Task BatchDeleteAsync(string ids)
        {
            try
            {
                foreach (var id in ids.Split(','))
                {
                    await ActivityRepository.DeleteAsync(kv => kv.Id == int.Parse(id));
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

    public class ActivityEditDto : EntityDto
    {
        /// <summary>
        /// 活动参数英文
        /// </summary>
        public string EOption { get; set; }
        /// <summary>
        /// 活动参数
        /// </summary>
        public string Option { get; set; }

        /// <summary>
        /// 0 活动 1票务 2最新消息
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 0 有效 1过期
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 活动标题
        /// </summary>
        [Required]
        public string Title { get; set; }

        /// <summary>
        /// 封面图
        /// </summary>
        public string ImgUrl { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [StringLength(500)]
        public string Desc { get; set; }

        /// <summary>
        /// 轮播图
        /// </summary>
        public string Imgs { get; set; }

        /// <summary>
        /// 活动详情2
        /// </summary>
        public string BackDetail { get; set; }

        /// <summary>
        /// 活动详情1
        /// </summary>
        public string PerDetail { get; set; }

        /// <summary>
        /// 简介
        /// </summary>
        public string IntroDetail { get; set; }

        /// <summary>
        /// 移动端封面图
        /// </summary>
        public string MobileImgUrl { get; set; }

        /// <summary>
        /// 缩略图
        /// </summary>
        public string ThumbnailUrl { get; set; }

        /// <summary>
        /// 移动端缩略图
        /// </summary>
        public string MobileThumbnailUrl { get; set; }

        /// <summary>
        /// 移动端轮播图
        /// </summary>
        public string MobileImgs { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// 地点
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 表单名称
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// 活动标题英文
        /// </summary>
        public string ETitle { get; set; }

        /// <summary>
        /// 描述英文
        /// </summary>
        [StringLength(500)]
        public string EDesc { get; set; }

        /// <summary>
        /// 日期英文
        /// </summary>
        public string EDate { get; set; }

        /// <summary>
        /// 地点英文
        /// </summary>
        public string EAddress { get; set; }

        /// <summary>
        /// 表单名称英文
        /// </summary>
        public string EFromName { get; set; }

        /// <summary>
        /// 活动详情英文
        /// </summary>
        public string EBackDetail { get; set; }

        /// <summary>
        /// 活动详情英文
        /// </summary>
        public string EPerDetail { get; set; }

        /// <summary>
        /// 简介英文
        /// </summary>
        public string EIntroDetail { get; set; }
    }

    public class ActivityDto : EntityDto
    {
        /// <summary>
        /// 活动参数英文
        /// </summary>
        public string EOption { get; set; }
        /// <summary>
        /// 活动参数
        /// </summary>
        public string Option { get; set; }

        /// <summary>
        /// 0 活动 1票务 2最新消息
        /// </summary>
        public int Type { get; set; }

        public string TypeText
        {
            get
            {
                switch (this.Type)
                {
                    case 1:
                        return "票务";
                    case 2:
                        return "最新消息";
                    default:
                        return "活动";
                }
            }
        }

        /// <summary>
        /// 0 有效 1过期
        /// </summary>
        public int State { get; set; }

        public string StateText
        {
            get
            {
                switch (this.State)
                {
                    case 1:
                        return "过期";
                    default:
                        return "有效";
                }
            }
        }
        /// <summary>
        /// 活动标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 封面图
        /// </summary>
        public string ImgUrl { get; set; }

        /// <summary>
        /// 移动端封面图
        /// </summary>
        public string MobileImgUrl { get; set; }

        /// <summary>
        /// 缩略图
        /// </summary>
        public string ThumbnailUrl { get; set; }

        /// <summary>
        /// 移动端缩略图
        /// </summary>
        public string MobileThumbnailUrl { get; set; }

        /// <summary>
        /// 移动端轮播图
        /// </summary>
        [StringLength(4000)]
        public string MobileImgs { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        /// 轮播图
        /// </summary>
        public string Imgs { get; set; }

        /// <summary>
        /// 活动详情2
        /// </summary>
        public string BackDetail { get; set; }

        /// <summary>
        /// 活动详情1
        /// </summary>
        public string PerDetail { get; set; }

        /// <summary>
        /// 简介
        /// </summary>
        public string IntroDetail { get; set; }

        /// <summary>
        /// 外链
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// 地点
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 表单名称
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// 活动标题英文
        /// </summary>
        public string ETitle { get; set; }

        /// <summary>
        /// 描述英文
        /// </summary>
        public string EDesc { get; set; }

        /// <summary>
        /// 日期英文
        /// </summary>
        public string EDate { get; set; }

        /// <summary>
        /// 地点英文
        /// </summary>
        public string EAddress { get; set; }

        /// <summary>
        /// 表单名称英文
        /// </summary>
        public string EFromName { get; set; }

        /// <summary>
        /// 活动详情英文
        /// </summary>
        public string EBackDetail { get; set; }

        /// <summary>
        /// 活动详情英文
        /// </summary>
        public string EPerDetail { get; set; }

        /// <summary>
        /// 简介英文
        /// </summary>
        public string EIntroDetail { get; set; }
    }
}
