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
using DevExtreme.AspNet.Data;
using Abp.Linq.Extensions;

namespace IF.Porsche
{
    public class AppointmentActivityAppService : IFAppServiceBase, IAppointmentActivityAppService
    {
        IRepository<AppointmentActivityAggregate> AppointmentActivityRepository;
        IRepository<StoreAggregate> StoreRepository;
        IRepository<StoreActivityAggregate> StoreActivityRepository;
        public AppointmentActivityAppService(
            IRepository<AppointmentActivityAggregate> AppointmentActivityRepository,
            IRepository<StoreAggregate> StoreRepository,
            IRepository<StoreActivityAggregate> StoreActivityRepository)
        {
            this.AppointmentActivityRepository = AppointmentActivityRepository;
            this.StoreRepository = StoreRepository;
            this.StoreActivityRepository = StoreActivityRepository;
        }
        private IEnumerable<AppointmentActivityDto> GetDataSource() {
            var result = from AppointmentActivity in AppointmentActivityRepository.GetAll().AsEnumerable()
                         join Store in StoreRepository.GetAll().AsEnumerable()
                         on AppointmentActivity.StoreId equals Store.Id
                         into Stores
                         from Store in Stores.DefaultIfEmpty()
                         join StoreActivity in StoreActivityRepository.GetAll().AsEnumerable()
                         on AppointmentActivity.ActivityId equals StoreActivity.Id
                         select new AppointmentActivityDto
                         {
                             Id = AppointmentActivity.Id,
                             Name = AppointmentActivity.Name,
                             ActivityId = AppointmentActivity.ActivityId,
                             CreationTime = AppointmentActivity.CreationTime,
                             Mobile = AppointmentActivity.Mobile,
                             SurName = AppointmentActivity.SurName,
                             UserId = AppointmentActivity.UserId,
                             Sex = AppointmentActivity.Sex,
                             StoreId = AppointmentActivity.StoreId,
                             StoreName = Store == null ? "" : Store.Name,
                             ActivityTitle = StoreActivity.Title,
                             State = AppointmentActivity.State,
                             ContactState = AppointmentActivity.ContactState,
                             Remarks = AppointmentActivity.Remarks
                         };
            return result;
        }

        #region 获取预约活动的相关信息，支持分页查询
        /// <summary>
        /// 获取预约活动的相关信息，支持分页查询
        /// </summary>
        /// <param name="loadOptions">查询参数</param>
        /// <returns></returns>
        public async Task<object> GetListDataAsync(DataSourceLoadOptions loadOptions)
        {
            return base.DataSourceLoadMap(GetDataSource(), loadOptions);
        }
        #endregion

        #region 获取我的预约活动的相关信息，支持分页查询
        /// <summary>
        /// 获取我的预约活动的相关信息，支持分页查询
        /// </summary>
        /// <param name="loadOptions">查询参数</param>
        /// <returns></returns>
        public async Task<object> GetMyListDataAsync(DataSourceLoadOptions loadOptions)
        {
            return base.DataSourceLoadMap(GetDataSource().Where(p => p.UserId == AbpSession.UserId.Value), loadOptions);
        }
        #endregion



        #region 根据ID获取预约活动信息
        /// <summary>
        /// 根据ID获取预约活动信息
        /// </summary>
        /// <param name="id">预约活动信息主键</param>
        /// <returns></returns>
        public async Task<AppointmentActivityDto> GetAsync(int id)
        {
            try
            {
                var AppointmentActivity = await AppointmentActivityRepository.GetAll().Where(k => k.Id == id).FirstOrDefaultAsync();

                if (AppointmentActivity == null)
                {
                    throw new EntityNotFoundException(typeof(AppointmentActivityAggregate), id);
                }
                var dto = this.ObjectMapper.Map<AppointmentActivityDto>(AppointmentActivity);
                return dto;
            }
            catch (Exception e)
            {
                Logger.Error("预约活动获取异常！", e);
                throw new AbpException("预约活动获取异常", e);
            }
        }
        #endregion

        #region  新增或者更改预约活动信息
        /// <summary>
        /// 新增或者更改预约活动信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task SaveAsync(AppointmentActivityEditDto input)
        {
            try
            {
                AppointmentActivityAggregate entity = this.ObjectMapper.Map<AppointmentActivityAggregate>(input);
                entity.UserId = AbpSession.UserId.Value;
                if (input.Id != 0)
                {
                    AppointmentActivityAggregate AppointmentActivityAggregate = AppointmentActivityRepository.GetAll().Where(k => k.Id == input.Id).FirstOrDefault();
                    AppointmentActivityAggregate.Name = entity.Name;
                    AppointmentActivityAggregate.ActivityId = entity.ActivityId;
                    AppointmentActivityAggregate.Mobile = entity.Mobile;
                    AppointmentActivityAggregate.Sex = entity.Sex;
                    AppointmentActivityAggregate.StoreId = entity.StoreId;
                    AppointmentActivityAggregate.SurName = entity.SurName;
                    await AppointmentActivityRepository.UpdateAsync(AppointmentActivityAggregate);
                }
                else
                {
                    await AppointmentActivityRepository.InsertAsync(entity);
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Logger.Error("预约活动保存异常！", e);
                throw new AbpException("预约活动保存异常！", e);
            }
        }
        #endregion

        #region  更改预约活动信息
        /// <summary>
        /// 更改预约活动信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [AbpAuthorize("Admin.AppointmentActivity.Edit")]
        public async Task AdminSaveAsync(AppointmentActivityEditDto input)
        {
            input.StoreId = ExportHelper.GetStoreId(input.StoreId);
            try
            {
                AppointmentActivityAggregate entity = this.ObjectMapper.Map<AppointmentActivityAggregate>(input);
                if (input.Id != 0)
                {
                    AppointmentActivityAggregate AppointmentActivityAggregate = AppointmentActivityRepository.GetAll().Where(k => k.Id == input.Id).FirstOrDefault();
                    AppointmentActivityAggregate.State = entity.State;
                    AppointmentActivityAggregate.ContactState = entity.ContactState;
                    AppointmentActivityAggregate.Remarks = entity.Remarks;
                    await AppointmentActivityRepository.UpdateAsync(AppointmentActivityAggregate);
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Logger.Error("预约活动保存异常！", e);
                throw new AbpException("预约活动保存异常！", e);
            }
        }
        #endregion

        #region 新增预约活动的默认参数
        /// <summary>
        /// 新增预约活动的默认参数
        /// </summary>
        /// <returns></returns>
        public async Task<AppointmentActivityEditDto> GetEmptyEntityAsync()
        {
            return await Task.Run(() => new AppointmentActivityEditDto());
        }
        #endregion

        #region 批量删除预约活动
        /// <summary>
        /// 批量删除预约活动
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete]
        //[AbpAuthorize("Admin.AppointmentActivity.Delete")]
        public async Task BatchDeleteAsync(string ids)
        {
            try
            {
                foreach (var id in ids.Split(','))
                {
                    await AppointmentActivityRepository.DeleteAsync(kv => kv.Id == int.Parse(id));
                }
                await CurrentUnitOfWork.SaveChangesAsync();

            }
            catch (Exception e)
            {
                Logger.Error("预约活动删除异常！", e);
                throw new AbpException("预约活动删除异常！", e);
            }
        }
        #endregion

        #region 获取统计
        /// <summary>
        /// 获取统计
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<object> Total(int id)
        {

            //var list = AppointmentActivityRepository.GetAll().WhereIf(id != 0, p => p.IsDeleted == false && p.StoreId == id).ToList();
            var list = AppointmentActivityRepository.GetAll().AsEnumerable();
            if (id != 0)
                list = list.Where(p => p.IsDeleted == false && p.StoreId == id);
            return new {
                n = "活动预约",
                n1 = "有效预约数",
                n2 = "已联系",
                n3 = "未联系",
                t1 = list.Where(p => p.State == 1).Count(), t2 = list.Where(p => p.ContactState == 3).Count(), t3 = list.Where(p => p.ContactState == 0).Count() };
        }
        #endregion
    }

    public class AppointmentActivityEditDto : EntityDto
    {
        /// <summary>
        /// 活动Id
        /// </summary>
        [Required]
        public int ActivityId { get; set; }

        /// <summary>
        /// 门店Id
        /// </summary>
        [Required]
        public int StoreId { get; set; }

        /// <summary>
        /// 性别 0男 1女
        /// </summary>
        public int Sex { get; set; }

        /// <summary>
        /// 姓
        /// </summary>
        public string SurName { get; set; }

        /// <summary>
        /// 名
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// 手机号
        /// </summary>
        [Required]
        public string Mobile { get; set; }

    }

    public class AdminAppointmentActivityEditDto : EntityDto
    {
        public int State { get; set; }

        public int ContactState { get; set; }

        public string Remarks { get; set; }

    }

    public class AppointmentActivityDto : EntityDto
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public long UserId { get; set; }
        /// <summary>
        /// 活动Id
        /// </summary>
        public int ActivityId { get; set; }

        /// <summary>
        /// 活动标题
        /// </summary>
        public string ActivityTitle { get; set; }

        /// <summary>
        /// 门店Id
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// 门店名称
        /// </summary>
        public string StoreName { get; set; }

        /// <summary>
        /// 性别 0男 1女
        /// </summary>
        public int Sex { get; set; }

        /// <summary>
        /// 姓
        /// </summary>
        public string SurName { get; set; }

        /// <summary>
        /// 名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 提交时间
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// 状态 0有效/1跟进/2无效
        /// </summary>
        public int State { get; set; }

        public string StateText
        {
            get
            {
                switch (this.State)
                {
                    case 1: return "跟进";
                    case 2: return "无效";
                    default: return "有效";
                }
            }
        }

        /// <summary>
        /// 联系状态 0未联系/1联系失败/2延迟提醒/3确认预约
        /// </summary>
        public int ContactState { get; set; }

        public string ContactStateText
        {
            get
            {
                switch (this.ContactState)
                {
                    case 1:
                        return "联系失败";
                    case 2:
                        return "延迟提醒";
                    case 3:
                        return "确认预约";
                    default:
                        return "未联系";
                }
            }
        }

        public string Remarks { get; set; }
    }
}
