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
using Abp.BackgroundJobs;
using Hangfire;
using DevExtreme.AspNet.Data;

namespace IF.Porsche
{
    public class AppointmentSmsAppService : IFAppServiceBase, IAppointmentSmsAppService
    {
        IRepository<AppointmentSmsAggregate> AppointmentSmsRepository;
        IRepository<AppointmentAggregate> AppointmentRepository;
        IRepository<AppointmentTestDriveAggregate> AppointmentTestDriveRepository;
        IRepository<User,long> UserRepository;
        IRepository<CarAggregate> CarRepository;
        IRepository<StoreAggregate> StoreRepository;
        IBackgroundJobManager _backgroundJobManager;
        
        public AppointmentSmsAppService(
            IRepository<AppointmentSmsAggregate> AppointmentSmsRepository,
            IRepository<User, long> UserRepository,
            IRepository<CarAggregate> CarRepository,
            IRepository<StoreAggregate> StoreRepository,
            IRepository<AppointmentAggregate> AppointmentRepository,
            IRepository<AppointmentTestDriveAggregate> AppointmentTestDriveRepository,
            IBackgroundJobManager _backgroundJobManager
            )
        {
            this.AppointmentSmsRepository = AppointmentSmsRepository;
            this.UserRepository = UserRepository;
            this.CarRepository = CarRepository;
            this.StoreRepository = StoreRepository;
            this.AppointmentRepository = AppointmentRepository;
            this.AppointmentTestDriveRepository = AppointmentTestDriveRepository;
            this._backgroundJobManager = _backgroundJobManager;
        }
        /// <summary>
        ///  获取短信提醒的相关信息，支持分页查询
        /// </summary>
        /// <param name="loadOptions">查询参数</param>
        /// <returns></returns>
        public async Task<object> GetListDataAsync(DataSourceLoadOptions loadOptions)
        {
            var result = from AppointmentSms in AppointmentSmsRepository.GetAll().AsEnumerable()
                         join user in UserRepository.GetAll().AsEnumerable()
                         on AppointmentSms.UserId equals user.Id
                         select new AppointmentSmsDto
                         {
                             Id = AppointmentSms.Id,
                             UserId = AppointmentSms.UserId,
                             Mobile = user.Mobile,
                             FullName = user.Surname + user.Name,
                             Msg = AppointmentSms.Msg,
                             IsAlreadyRead = AppointmentSms.IsAlreadyRead,
                             CreationTime = AppointmentSms.CreationTime
                         };
            loadOptions.Sort = new SortingInfo[] { new SortingInfo() { Selector = "CreationTime", Desc = true } };
            return base.DataSourceLoadMap(result, loadOptions);
        }

        /// <summary>
        ///  获取短信提醒的相关信息，支持分页查询
        /// </summary>
        /// <param name="loadOptions">查询参数</param>
        /// <returns></returns>
        public async Task<object> GetMyListDataAsync(DataSourceLoadOptions loadOptions)
        {
            var result = from AppointmentSms in AppointmentSmsRepository.GetAll().AsEnumerable()
                         join user in UserRepository.GetAll().AsEnumerable()
                         on AppointmentSms.UserId equals user.Id
                         select new AppointmentSmsDto
                         {
                             Id = AppointmentSms.Id,
                             UserId = AppointmentSms.UserId,
                             Mobile = user.Mobile,
                             FullName = user.UserName,
                             Msg = AppointmentSms.Msg,
                             IsAlreadyRead = AppointmentSms.IsAlreadyRead,
                             CreationTime = AppointmentSms.CreationTime
                         };
            loadOptions.Sort = new SortingInfo[] { new SortingInfo() { Selector = "CreationTime", Desc = true } };
            return base.DataSourceLoadMap(result.Where(p => p.UserId == AbpSession.UserId.Value), loadOptions);
        }

        #region 根据ID获取短信提醒信息
        /// <summary>
        /// 根据ID获取短信提醒信息
        /// </summary>
        /// <param name="id">短信提醒信息主键</param>
        /// <returns></returns>
        public async Task<AppointmentSmsDto> GetAsync(int id)
        {
            try
            {
                var AppointmentSms = await AppointmentSmsRepository.GetAll().Where(k => k.Id == id).FirstOrDefaultAsync();

                if (AppointmentSms == null)
                {
                    throw new EntityNotFoundException(typeof(AppointmentSmsAggregate), id);
                }
                var dto = this.ObjectMapper.Map<AppointmentSmsDto>(AppointmentSms);
                return dto;
            }
            catch (Exception e)
            {
                Logger.Error("短信提醒获取异常！", e);
                throw new AbpException("短信提醒获取异常", e);
            }
        }
        #endregion

        #region  新增或者更改短信提醒信息
        /// <summary>
        /// 新增或者更改短信提醒信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task SaveAsync(AppointmentSmsEditDto input)
        {
            try
            {
                AppointmentSmsAggregate entity = this.ObjectMapper.Map<AppointmentSmsAggregate>(input);
                if (input.Id != 0)
                {
                    AppointmentSmsAggregate AppointmentSmsAggregate = AppointmentSmsRepository.FirstOrDefault(k => k.Id == input.Id && k.UserId == AbpSession.UserId.Value);
                    AppointmentSmsAggregate.IsAlreadyRead = entity.IsAlreadyRead;
                    await AppointmentSmsRepository.UpdateAsync(AppointmentSmsAggregate);
                }
                else
                {
                    //await AppointmentSmsRepository.InsertAsync(entity);
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Logger.Error("短信提醒保存异常！", e);
                throw new AbpException("短信提醒保存异常！", e);
            }
        }
        #endregion

        #region 新增短信提醒的默认参数
        /// <summary>
        /// 新增短信提醒的默认参数
        /// </summary>
        /// <returns></returns>
        public async Task<AppointmentSmsEditDto> GetEmptyEntityAsync()
        {
            return await Task.Run(() => new AppointmentSmsEditDto());
        }
        #endregion

        #region 批量删除短信提醒
        /// <summary>
        /// 批量删除短信提醒
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete]
        [AbpAuthorize("Admin.AppointmentSms.Edit")]
        public async Task BatchDeleteAsync(string ids)
        {
            try
            {
                foreach (var id in ids.Split(','))
                {
                    await AppointmentSmsRepository.DeleteAsync(kv => kv.Id == int.Parse(id));
                }
                await CurrentUnitOfWork.SaveChangesAsync();

            }
            catch (Exception e)
            {
                Logger.Error("短信提醒删除异常！", e);
                throw new AbpException("短信提醒删除异常！", e);
            }
        }
        #endregion

        public async Task Start()
        {
            //await _backgroundJobManager.EnqueueAsync<SmsBackgroundProcess,int>(1);
            RecurringJob.AddOrUpdate<IMakeInactiveSmsWorker>(a => a.DoWork(), Cron.Daily);
        }

        
    }

    public class AppointmentSmsEditDto : EntityDto
    {
        /// <summary>
        /// 是否已读
        /// </summary>
        public bool IsAlreadyRead { get; set; }
    }

    public class AppointmentSmsDto : EntityDto
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 用户手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 用户名称
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// 是否已读
        /// </summary>
        public bool IsAlreadyRead { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string StateText { get { return this.IsAlreadyRead ? "已读" : "未读"; } }

        /// <summary>
        /// 提醒信息
        /// </summary>
        public string Msg { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
