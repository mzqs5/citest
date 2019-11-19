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

namespace IF.Porsche
{
    public class AppointmentWebActivityAppService : IFAppServiceBase, IAppointmentWebActivityAppService
    {
        IRepository<AppointmentWebActivityAggregate> AppointmentRepository;
        IRepository<ActivityAggregate> ActivityRepository;
        IRepository<User, long> UserRepository;
        public AppointmentWebActivityAppService(
            IRepository<AppointmentWebActivityAggregate> AppointmentWebActivityRepository,
            IRepository<User, long> UserRepository,
            IRepository<ActivityAggregate> ActivityRepository)
        {
            this.AppointmentRepository = AppointmentWebActivityRepository;
            this.UserRepository = UserRepository;
            this.ActivityRepository = ActivityRepository;
        }

        #region 获取网站活动预约的相关信息，支持分页查询
        /// <summary>
        /// 获取网站活动预约的相关信息，支持分页查询
        /// </summary>
        /// <param name="loadOptions">查询参数</param>
        /// <returns></returns>
        public async Task<object> GetListDataAsync(DataSourceLoadOptions loadOptions)
        {
            var result = from Appointment in AppointmentRepository.GetAll().AsEnumerable()
                         join Activity in ActivityRepository.GetAll().AsEnumerable()
                         on Appointment.ActivityId equals Activity.Id
                         select new AppointmentWebActivityDto
                         {
                             Id = Appointment.Id,
                             Name = Appointment.Name,
                             Option = Appointment.Option,
                             Mobile = Appointment.Mobile,
                             SurName = Appointment.SurName,
                             UserId = Appointment.UserId,
                             State = Appointment.State,
                             ContactState = Appointment.ContactState,
                             CreationTime = Appointment.CreationTime,
                             ActivityType = Activity.Type,
                             Remarks = Appointment.Remarks,
                             ActivityId = Appointment.ActivityId,
                             ActivityTitle = Activity.Title,
                             ActivityAddress = Activity.Address,
                             EActivityTitle=Activity.ETitle,
                             EActivityAddress=Activity.EAddress
                         };
            loadOptions.Sort = new SortingInfo[] { new SortingInfo() { Selector = "CreationTime", Desc = true } };
            return base.DataSourceLoadMap(result, loadOptions);
        }
        #endregion

        #region 获取我的网站活动预约的相关信息，支持分页查询
        /// <summary>
        /// 获取我的网站活动预约的相关信息，支持分页查询
        /// </summary>
        /// <param name="loadOptions">查询参数</param>
        /// <returns></returns>
        public async Task<object> GetMyListDataAsync(DataSourceLoadOptions loadOptions)
        {
            var result = from AppointmentWebActivity in AppointmentRepository.GetAll()
                         .Where(p => p.UserId == AbpSession.UserId.Value).AsEnumerable()
                         join Activity in ActivityRepository.GetAll().AsEnumerable()
                         on AppointmentWebActivity.ActivityId equals Activity.Id
                         select new AppointmentWebActivityDto
                         {
                             Id = AppointmentWebActivity.Id,
                             Name = AppointmentWebActivity.Name,
                             Option = AppointmentWebActivity.Option,
                             Mobile = AppointmentWebActivity.Mobile,
                             SurName = AppointmentWebActivity.SurName,
                             UserId = AppointmentWebActivity.UserId,
                             State = AppointmentWebActivity.State,
                             ContactState = AppointmentWebActivity.ContactState,
                             CreationTime = AppointmentWebActivity.CreationTime,
                             Remarks = AppointmentWebActivity.Remarks,
                             ActivityId = AppointmentWebActivity.ActivityId,
                             ActivityTitle = Activity.Title,
                             ActivityAddress = Activity.Address,
                             EActivityTitle = Activity.ETitle,
                             EActivityAddress = Activity.EAddress
                         };
            loadOptions.Sort = new SortingInfo[] { new SortingInfo() { Selector = "CreationTime", Desc = true } };
            return base.DataSourceLoadMap(result, loadOptions);
        }
        #endregion

        #region 根据ID获取网站活动预约信息
        /// <summary>
        /// 根据ID获取网站活动预约信息
        /// </summary>
        /// <param name="id">网站活动预约信息主键</param>
        /// <returns></returns>
        public async Task<AppointmentWebActivityDto> GetAsync(int id)
        {
            try
            {
                var AppointmentWebActivity = await AppointmentRepository.GetAll().Where(k => k.Id == id).FirstOrDefaultAsync();

                if (AppointmentWebActivity == null)
                {
                    throw new EntityNotFoundException(typeof(AppointmentWebActivityAggregate), id);
                }
                var dto = this.ObjectMapper.Map<AppointmentWebActivityDto>(AppointmentWebActivity);
                return dto;
            }
            catch (Exception e)
            {
                Logger.Error("网站活动预约获取异常！", e);
                throw new AbpException("网站活动预约获取异常", e);
            }
        }
        #endregion

        #region  新增或者更改网站活动预约信息
        /// <summary>
        /// 新增或者更改网站活动预约信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task SaveAsync(AppointmentWebActivityEditDto input)
        {
            try
            {
                AppointmentWebActivityAggregate entity = this.ObjectMapper.Map<AppointmentWebActivityAggregate>(input);
                entity.UserId = AbpSession.UserId.Value;
                var user = await UserRepository.GetAll().Include(p => p.UserCars).FirstOrDefaultAsync(p => p.Id == AbpSession.UserId.Value);
                user.Name = input.Name;
                user.Surname = input.SurName;
                await UserRepository.UpdateAsync(user);
                if (input.Id != 0)
                {
                    AppointmentWebActivityAggregate AppointmentWebActivityAggregate = AppointmentRepository.GetAll().Where(k => k.Id == input.Id).FirstOrDefault();
                    AppointmentWebActivityAggregate.ActivityId = entity.ActivityId;
                    AppointmentWebActivityAggregate.SurName = entity.SurName;
                    AppointmentWebActivityAggregate.Name = entity.Name;
                    AppointmentWebActivityAggregate.Mobile = entity.Mobile;
                    AppointmentWebActivityAggregate.Option = entity.Option;

                    await AppointmentRepository.UpdateAsync(AppointmentWebActivityAggregate);
                }
                else
                {
                    await AppointmentRepository.InsertAsync(entity);
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Logger.Error("网站活动预约保存异常！", e);
                throw new AbpException("网站活动预约保存异常！", e);
            }
        }
        #endregion

        #region  更改网站活动预约信息
        /// <summary>
        /// 更改网站活动预约信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [AbpAuthorize("Admin.AppointmentWebActivity.Edit")]
        public async Task AdminSaveAsync(AdminAppointmentWebActivityEditDto input)
        {
            try
            {
                AppointmentWebActivityAggregate entity = this.ObjectMapper.Map<AppointmentWebActivityAggregate>(input);
                if (input.Id != 0)
                {
                    AppointmentWebActivityAggregate AppointmentAggregate = AppointmentRepository.GetAll().Where(k => k.Id == input.Id).FirstOrDefault();
                    AppointmentAggregate.State = entity.State;
                    AppointmentAggregate.ContactState = entity.ContactState;
                    AppointmentAggregate.Remarks = entity.Remarks;
                    await AppointmentRepository.UpdateAsync(AppointmentAggregate);
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

        #region 新增网站活动预约的默认参数
        /// <summary>
        /// 新增网站活动预约的默认参数
        /// </summary>
        /// <returns></returns>
        public async Task<AppointmentWebActivityEditDto> GetEmptyEntityAsync()
        {
            return await Task.Run(() => new AppointmentWebActivityEditDto());
        }
        #endregion

        #region 批量删除网站活动预约
        /// <summary>
        /// 批量删除网站活动预约
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
                    await AppointmentRepository.DeleteAsync(kv => kv.Id == int.Parse(id));
                }
                await CurrentUnitOfWork.SaveChangesAsync();

            }
            catch (Exception e)
            {
                Logger.Error("网站活动预约删除异常！", e);
                throw new AbpException("网站活动预约删除异常！", e);
            }
        }
        #endregion

        #region 核销
        /// <summary>
        /// 核销
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AbpAuthorize("Admin.AppointmentWebActivity.WriteOff")]
        public async Task WriteOff(string ids)
        {
            try
            {
                foreach (var id in ids.Split(','))
                {
                    var entity = await AppointmentRepository.FirstOrDefaultAsync(p => p.Id == int.Parse(id));
                    entity.State = 1;
                    await AppointmentRepository.UpdateAsync(entity);
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Logger.Error("预约活动核销异常！", e);
                throw new AbpException("预约活动核销异常！", e);
            }
        }
        #endregion
    }

    public class AppointmentWebActivityEditDto : EntityDto
    {
        /// <summary>
        /// 姓
        /// </summary>
        [Required]
        [StringLength(20)]
        public string SurName { get; set; }

        /// <summary>
        /// 名
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Name { get; set; }

        /// <summary>
        /// 活动Id
        /// </summary>
        [Required]
        public int ActivityId { get; set; }


        /// <summary>
        /// 手机号
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Mobile { get; set; }

        /// <summary>
        /// 参数
        /// </summary>
        [StringLength(50)]
        public string Option { get; set; }
    }


    public class AdminAppointmentWebActivityEditDto : EntityDto
    {
        /// <summary>
        /// 状态 0有效/1跟进/2无效
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 联系状态 0未联系/1联系失败/2延迟提醒/3确认预约
        /// </summary>
        public int ContactState { get; set; }

        public string Remarks { get; set; }
    }
    public class AppointmentWebActivityDto : EntityDto
    {

        /// <summary>
        /// 活动Id
        /// </summary>
        public int ActivityId { get; set; }

        /// <summary>
        /// 活动类型
        /// </summary>
        public int ActivityType { get; set; }

        /// <summary>
        /// 活动标题
        /// </summary>
        public string ActivityTitle { get; set; }

        /// <summary>
        /// 活动地址
        /// </summary>
        public string ActivityAddress { get; set; }

        /// <summary>
        /// 活动标题 英文
        /// </summary>
        public string EActivityTitle { get; set; }

        /// <summary>
        /// 活动地址 英文
        /// </summary>
        public string EActivityAddress { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public long UserId { get; set; }

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
        /// 参数
        /// </summary>
        public string Option { get; set; }

        /// <summary>
        /// 创建时间
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
        /// 预约类型
        /// </summary>
        public string AppointmentType
        {
            get
            {
                switch (this.ActivityType)
                {
                    case 0:
                        return "WebActivity";
                    case 1:
                        return "Ticket";
                    default:
                        return "NewInfo";
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
