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
    public class AppointmentEuroDriveAppService : IFAppServiceBase, IAppointmentEuroDriveAppService
    {
        IRepository<AppointmentEuroDriveAggregate> AppointmentEuroDriveRepository;
        IRepository<User, long> UserRepository;
        public AppointmentEuroDriveAppService(
            IRepository<AppointmentEuroDriveAggregate> AppointmentEuroDriveRepository,
            IRepository<User, long> UserRepository)
        {
            this.AppointmentEuroDriveRepository = AppointmentEuroDriveRepository;
            this.UserRepository = UserRepository;
        }

        #region 获取预约追星欧洲的相关信息，支持分页查询
        /// <summary>
        /// 获取预约追星欧洲的相关信息，支持分页查询
        /// </summary>
        /// <param name="loadOptions">查询参数</param>
        /// <returns></returns>
        [AbpAuthorize("Pages.AppointmentEuroDrive")]
        public async Task<object> GetListDataAsync(DataSourceLoadOptions loadOptions)
        {
            var result = from AppointmentEuroDrive in AppointmentEuroDriveRepository.GetAll().AsEnumerable()
                         select new AppointmentEuroDriveDto
                         {
                             Id = AppointmentEuroDrive.Id,
                             Name = AppointmentEuroDrive.Name,
                             Option = AppointmentEuroDrive.Option,
                             Mobile = AppointmentEuroDrive.Mobile,
                             SurName = AppointmentEuroDrive.SurName,
                             UserId = AppointmentEuroDrive.UserId,
                             CreationTime = AppointmentEuroDrive.CreationTime
                         };
            loadOptions.Sort = new SortingInfo[] { new SortingInfo() { Selector = "CreationTime", Desc = true } };
            return base.DataSourceLoadMap(result, loadOptions);
        }
        #endregion

        #region 获取我的预约追星欧洲的相关信息，支持分页查询
        /// <summary>
        /// 获取我的预约追星欧洲的相关信息，支持分页查询
        /// </summary>
        /// <param name="loadOptions">查询参数</param>
        /// <returns></returns>
        public async Task<object> GetMyListDataAsync(DataSourceLoadOptions loadOptions)
        {
            var result = from AppointmentEuroDrive in AppointmentEuroDriveRepository.GetAll()
                         .Where(p => p.UserId == AbpSession.UserId.Value).AsEnumerable()
                         select new AppointmentEuroDriveDto
                         {
                             Id = AppointmentEuroDrive.Id,
                             Name = AppointmentEuroDrive.Name,
                             Option = AppointmentEuroDrive.Option,
                             Mobile = AppointmentEuroDrive.Mobile,
                             SurName = AppointmentEuroDrive.SurName,
                             UserId = AppointmentEuroDrive.UserId,
                             CreationTime = AppointmentEuroDrive.CreationTime
                         };
            loadOptions.Sort = new SortingInfo[] { new SortingInfo() { Selector = "CreationTime", Desc = true } };
            return base.DataSourceLoadMap(result, loadOptions);
        }
        #endregion

        #region 根据ID获取预约追星欧洲信息
        /// <summary>
        /// 根据ID获取预约追星欧洲信息
        /// </summary>
        /// <param name="id">预约追星欧洲信息主键</param>
        /// <returns></returns>
        public async Task<AppointmentEuroDriveDto> GetAsync(int id)
        {
            try
            {
                var AppointmentEuroDrive = await AppointmentEuroDriveRepository.GetAll().Where(k => k.Id == id).FirstOrDefaultAsync();

                if (AppointmentEuroDrive == null)
                {
                    throw new EntityNotFoundException(typeof(AppointmentEuroDriveAggregate), id);
                }
                var dto = this.ObjectMapper.Map<AppointmentEuroDriveDto>(AppointmentEuroDrive);
                return dto;
            }
            catch (Exception e)
            {
                Logger.Error("预约追星欧洲获取异常！", e);
                throw new AbpException("预约追星欧洲获取异常", e);
            }
        }
        #endregion

        #region  新增或者更改预约追星欧洲信息
        /// <summary>
        /// 新增或者更改预约追星欧洲信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task SaveAsync(AppointmentEuroDriveEditDto input)
        {
            try
            {
                AppointmentEuroDriveAggregate entity = this.ObjectMapper.Map<AppointmentEuroDriveAggregate>(input);
                entity.UserId = AbpSession.UserId.Value;
                var user = await UserRepository.GetAll().Include(p => p.UserCars).FirstOrDefaultAsync(p => p.Id == AbpSession.UserId.Value);
                user.Name = input.Name;
                user.Surname = input.SurName;
                await UserRepository.UpdateAsync(user);
                if (input.Id != 0)
                {
                    AppointmentEuroDriveAggregate AppointmentEuroDriveAggregate = AppointmentEuroDriveRepository.GetAll().Where(k => k.Id == input.Id).FirstOrDefault();
                    AppointmentEuroDriveAggregate.SurName = entity.SurName;
                    AppointmentEuroDriveAggregate.Name = entity.Name;
                    AppointmentEuroDriveAggregate.Mobile = entity.Mobile;
                    AppointmentEuroDriveAggregate.Option = entity.Option;

                    await AppointmentEuroDriveRepository.UpdateAsync(AppointmentEuroDriveAggregate);
                }
                else
                {
                    await AppointmentEuroDriveRepository.InsertAsync(entity);
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            catch (Exception e)
            {
                Logger.Error("预约追星欧洲保存异常！", e);
                throw new AbpException("预约追星欧洲保存异常！", e);
            }
        }
        #endregion

        #region 新增预约追星欧洲的默认参数
        /// <summary>
        /// 新增预约追星欧洲的默认参数
        /// </summary>
        /// <returns></returns>
        public async Task<AppointmentEuroDriveEditDto> GetEmptyEntityAsync()
        {
            return await Task.Run(() => new AppointmentEuroDriveEditDto());
        }
        #endregion

        #region 批量删除预约追星欧洲
        /// <summary>
        /// 批量删除预约追星欧洲
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
                    await AppointmentEuroDriveRepository.DeleteAsync(kv => kv.Id == int.Parse(id));
                }
                await CurrentUnitOfWork.SaveChangesAsync();

            }
            catch (Exception e)
            {
                Logger.Error("预约追星欧洲删除异常！", e);
                throw new AbpException("预约追星欧洲删除异常！", e);
            }
        }
        #endregion
    }

    public class AppointmentEuroDriveEditDto : EntityDto
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

    public class AppointmentEuroDriveDto : EntityDto
    {
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
        /// 预约类型
        /// </summary>
        public string AppointmentType { get { return "EuroDrive"; } }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }
    }
}
