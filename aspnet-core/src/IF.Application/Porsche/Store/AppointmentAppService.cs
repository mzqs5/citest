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
    public class AppointmentAppService : IFAppServiceBase, IAppointmentActivityAppService
    {
        IRepository<AppointmentAggregate> AppointmentRepository;
        IRepository<CarAggregate> CarRepository;
        IRepository<User, long> UserRepository;
        IRepository<StoreAggregate> StoreRepository;
        IRepository<AppointmentWebActivityAggregate> AppointmentWebActivityRepository;
        IRepository<ActivityAggregate> ActivityRepository;
        IRepository<AppointmentTestDriveAggregate> AppointmentTestDriveRepository;
        public AppointmentAppService(
            IRepository<AppointmentAggregate> AppointmentRepository,
            IRepository<CarAggregate> CarRepository,
            IRepository<User, long> UserRepository,
            IRepository<StoreAggregate> StoreRepository,
            IRepository<ActivityAggregate> ActivityRepository,
            IRepository<AppointmentWebActivityAggregate> AppointmentWebActivityRepository,
            IRepository<AppointmentTestDriveAggregate> AppointmentTestDriveRepository)
        {
            this.AppointmentRepository = AppointmentRepository;
            this.CarRepository = CarRepository;
            this.ActivityRepository = ActivityRepository;
            this.UserRepository = UserRepository;
            this.StoreRepository = StoreRepository;
            this.AppointmentWebActivityRepository = AppointmentWebActivityRepository;
            this.AppointmentTestDriveRepository = AppointmentTestDriveRepository;
        }
        private IEnumerable<AppointmentDto> GetDataSource()
        {
            var UserCars = UserRepository.GetAll().Include(p => p.UserCars).SelectMany(p => p.UserCars);
            var result = from Appointment in AppointmentRepository.GetAll().AsEnumerable()
                         join UserCar in UserCars
                         on Appointment.UserCarId equals UserCar.Id
                         join Store in StoreRepository.GetAll().AsEnumerable()
                         on Appointment.StoreId equals Store.Id
                         into Stores
                         from Store in Stores.DefaultIfEmpty()
                         select new AppointmentDto
                         {
                             Id = Appointment.Id,
                             Name = Appointment.Name,
                             Date = Appointment.Date,
                             Mobile = Appointment.Mobile,
                             StoreId = Appointment.StoreId,
                             UserCarId = Appointment.UserCarId,
                             CarId = UserCar.CarId,
                             CarName = UserCar.CarName,
                             FrameNo = UserCar.FrameNo,
                             StoreName = Store==null?"": Store.Name,
                             StoreAddress = Store == null ? "" : Store.Address,
                             StorePhone = Store == null ? "" : Store.Phone,
                             EStoreName = Store == null ? "" : Store.EName,
                             EStoreAddress = Store == null ? "" : Store.EAddress,
                             EStorePhone = Store == null ? "" : Store.EPhone,
                             SurName = Appointment.SurName,
                             UserId = Appointment.UserId,
                             Sex = Appointment.Sex,
                             CreationTime = Appointment.CreationTime,
                             City = Appointment.City,
                             Area = Appointment.Area,
                             Street = Appointment.Street,
                             DetailAddress = Appointment.DetailAddress,
                             State = Appointment.State,
                             ContactState = Appointment.ContactState,
                             Remarks = Appointment.Remarks,
                             Type = Appointment.Type,
                             IsDoor = Appointment.IsDoor
                         };
            return result;
        }

        #region 获取预约服务的相关信息，支持分页查询
        /// <summary>
        /// 获取预约服务的相关信息，支持分页查询
        /// </summary>
        /// <param name="loadOptions">查询参数</param>
        /// <returns></returns>
        public async Task<object> GetListDataAsync(DataSourceLoadOptions loadOptions)
        {
            loadOptions.Sort = new SortingInfo[] { new SortingInfo() { Selector = "CreationTime", Desc = true } };
            return base.DataSourceLoadMap(GetDataSource(), loadOptions);
        }
        #endregion

        #region 获取我的预约服务的相关信息，支持分页查询
        /// <summary>
        /// 获取我的预约服务的相关信息，支持分页查询
        /// </summary>
        /// <param name="loadOptions">查询参数</param>
        /// <returns></returns>
        public async Task<object> GetMyListDataAsync(DataSourceLoadOptions loadOptions)
        {
            loadOptions.Sort = new SortingInfo[] { new SortingInfo() { Selector = "CreationTime", Desc = true } };
            return base.DataSourceLoadMap(GetDataSource().Where(p => p.UserId == AbpSession.UserId.Value), loadOptions);
        }
        #endregion

        #region 获取我的所有预约，支持分页查询

        private IEnumerable<AppointmentTestDriveDto> GetDataSourceTestDrive()
        {
            var result = from Appointment in AppointmentTestDriveRepository.GetAll().AsEnumerable()
                         join Car in CarRepository.GetAll().AsEnumerable()
                         on Appointment.CarId equals Car.Id
                         join Store in StoreRepository.GetAll().AsEnumerable()
                         on Appointment.StoreId equals Store.Id
                         select new AppointmentTestDriveDto
                         {
                             Id = Appointment.Id,
                             Name = Appointment.Name,
                             Mobile = Appointment.Mobile,
                             CarId = Appointment.CarId,
                             CarName = Car.Name,
                             StoreId = Appointment.StoreId,
                             StoreName = Store.Name,
                             StoreAddress = Store.Address,
                             StorePhone = Store.Phone,
                             EStoreName = Store.EName,
                             EStoreAddress = Store.EAddress,
                             EStorePhone = Store.EPhone,
                             SurName = Appointment.SurName,
                             UserId = Appointment.UserId,
                             Sex = Appointment.Sex,
                             Date = Appointment.Date,
                             CreationTime = Appointment.CreationTime,
                             State = Appointment.State,
                             Remarks = Appointment.Remarks
                         };
            return result;
        }

        private IEnumerable<AppointmentWebActivityDto> GetDataSourceActivity()
        {
            var result = from Appointment in AppointmentWebActivityRepository.GetAll().AsEnumerable()
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
                             Remarks = Appointment.Remarks,
                             ActivityType = Activity.Type,
                             ActivityId = Appointment.ActivityId,
                             ActivityTitle = Activity.Title,
                             ActivityAddress = Activity.Address,
                             EActivityTitle = Activity.ETitle,
                             EActivityAddress = Activity.EAddress
                         };
            return result;
        }

        /// <summary>
        /// 获取我的所有预约，支持分页查询
        /// </summary>
        /// <param name="loadOptions">查询参数</param>
        /// <returns></returns>
        public async Task<object> GetMyAllListDataAsync(DataSourceLoadOptions loadOptions)
        {
            List<AllAppointmentDto> list = new List<AllAppointmentDto>();
            list.AddRange(this.ObjectMapper.Map<List<AllAppointmentDto>>(GetDataSource().Where(p => p.UserId == AbpSession.UserId.Value).ToList()));
            list.AddRange(this.ObjectMapper.Map<List<AllAppointmentDto>>(GetDataSourceActivity().Where(p => p.UserId == AbpSession.UserId.Value).ToList()));
            list.AddRange(this.ObjectMapper.Map<List<AllAppointmentDto>>(GetDataSourceTestDrive().Where(p => p.UserId == AbpSession.UserId.Value).ToList()));
            return base.DataSourceLoadMap(list.OrderByDescending(p => p.CreationTime), loadOptions);
        }
        #endregion

        #region 根据ID获取预约服务信息
        /// <summary>
        /// 根据ID获取预约服务信息
        /// </summary>
        /// <param name="id">预约服务信息主键</param>
        /// <returns></returns>
        public async Task<AppointmentDto> GetAsync(int id)
        {
            try
            {
                var Appointment = await AppointmentRepository.GetAll().Where(k => k.Id == id).FirstOrDefaultAsync();

                if (Appointment == null)
                {
                    throw new EntityNotFoundException(typeof(AppointmentAggregate), id);
                }
                var dto = this.ObjectMapper.Map<AppointmentDto>(Appointment);
                var UserCars = await UserRepository.GetAll().Include(p => p.UserCars).SelectMany(p => p.UserCars).FirstOrDefaultAsync(p => p.Id == Appointment.UserCarId);
                if (UserCars != null)
                {
                    dto.CarId = UserCars.CarId;
                    dto.CarName = UserCars.CarName;
                    dto.FrameNo = UserCars.FrameNo;
                }
                var Store = await StoreRepository.FirstOrDefaultAsync(p => p.Id == Appointment.StoreId);
                if (Store != null)
                {
                    dto.StoreName = Store.Name;
                    dto.StorePhone = Store.Phone;
                    dto.StoreAddress = Store.Address;
                    dto.EStoreName = Store.EName;
                    dto.EStorePhone = Store.EPhone;
                    dto.EStoreAddress = Store.EAddress;
                }
                return dto;
            }
            catch (Exception e)
            {
                Logger.Error("预约服务获取异常！", e);
                throw new AbpException("预约服务获取异常", e);
            }
        }
        #endregion

        #region  新增或者更改预约服务信息
        /// <summary>
        /// 新增或者更改预约服务信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task SaveAsync(AppointmentEditDto input)
        {
            input.StoreId = ExportHelper.GetStoreId(input.StoreId);
            AppointmentAggregate entity = this.ObjectMapper.Map<AppointmentAggregate>(input);
            entity.UserId = AbpSession.UserId.Value;
            var user = await UserRepository.GetAll().Include(p => p.UserCars).FirstOrDefaultAsync(p => p.Id == AbpSession.UserId.Value);
            if (!user.UserCars.Any(p => p.Id == entity.UserCarId))
                throw new AbpException("非法车辆id");
            user.LastStoreId = input.StoreId;
            user.Name = input.Name;
            user.Surname = input.SurName;
            await UserRepository.UpdateAsync(user);
            if (input.Id != 0)
            {
                AppointmentAggregate AppointmentAggregate = AppointmentRepository.GetAll().Where(k => k.Id == input.Id).FirstOrDefault();
                AppointmentAggregate.Name = entity.Name;
                AppointmentAggregate.UserCarId = entity.UserCarId;
                AppointmentAggregate.SurName = entity.SurName;
                AppointmentAggregate.StoreId = entity.StoreId;
                AppointmentAggregate.Mobile = entity.Mobile;
                AppointmentAggregate.Date = entity.Date;
                AppointmentAggregate.Sex = entity.Sex;
                AppointmentAggregate.IsDoor = entity.IsDoor;
                AppointmentAggregate.Type = entity.Type;
                AppointmentAggregate.DetailAddress = entity.DetailAddress;
                AppointmentAggregate.Area = entity.Area;
                AppointmentAggregate.City = entity.City;
                AppointmentAggregate.Street = entity.Street;

                await AppointmentRepository.UpdateAsync(AppointmentAggregate);
            }
            else
            {
                await AppointmentRepository.InsertAsync(entity);
            }
            await CurrentUnitOfWork.SaveChangesAsync();
        }
        #endregion

        #region  修改预约服务信息 管理后台使用
        /// <summary>
        /// 修改预约服务信息 管理后台使用
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [AbpAuthorize("Admin.Appointment.Edit")]
        public async Task AdminSaveAsync(AdminAppointmentEditDto input)
        {
            try
            {
                if (input.Id != 0)
                {
                    AppointmentAggregate AppointmentAggregate = AppointmentRepository.FirstOrDefault(k => k.Id == input.Id);
                    AppointmentAggregate.State = input.State;
                    AppointmentAggregate.ContactState = input.ContactState;
                    AppointmentAggregate.Remarks = input.Remarks;

                    await AppointmentRepository.UpdateAsync(AppointmentAggregate);
                }

                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Logger.Error("预约试驾保存异常！", e);
                throw new AbpException("预约试驾保存异常！", e);
            }
        }
        #endregion

        #region 新增预约服务的默认参数
        /// <summary>
        /// 新增预约服务的默认参数
        /// </summary>
        /// <returns></returns>
        public async Task<AppointmentEditDto> GetEmptyEntityAsync()
        {
            return await Task.Run(() => new AppointmentEditDto());
        }
        #endregion

        #region 批量删除预约服务
        /// <summary>
        /// 批量删除预约服务
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete]
        [AbpAuthorize("Admin.Appointment.Edit")]
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
                Logger.Error("预约服务删除异常！", e);
                throw new AbpException("预约服务删除异常！", e);
            }
        }
        #endregion

        #region 核销
        /// <summary>
        /// 核销
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AbpAuthorize("Admin.Appointment.WriteOff")]
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
                Logger.Error("预约试驾核销异常！", e);
                throw new AbpException("预约试驾核销异常！", e);
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

            //var list = AppointmentRepository.GetAll().WhereIf(id != 0, p => p.IsDeleted == false && p.StoreId == id).ToList();
            var list = AppointmentRepository.GetAll().AsEnumerable();
            if (id != 0)
                list = list.Where(p => p.IsDeleted == false && p.StoreId == id);
            return new {
                n = "服务",
                n1 = "预约数",
                n2 = "完成数",
                n3 = "未完成数",
                t1 = list.Count(), t2 = list.Where(p => p.State == 1).Count(), t3 = list.Where(p => p.State == 0).Count() };
        }
        #endregion
    }

    internal class AllAppointmentDto : EntityDto
    {
        /// <summary>
        /// 性别
        /// </summary>
        public int Sex { get; set; }
        /// <summary>
        /// 用户Id
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 门店Id
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// 预约我的车辆
        /// </summary>
        public int UserCarId { get; set; }

        /// <summary>
        /// 车架号
        /// </summary>
        public string FrameNo { get; set; }

        /// <summary>
        /// 预约车型
        /// </summary>
        public int CarId { get; set; }

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
        /// 预约服务日期
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 活动地址
        /// </summary>
        public string ActivityAddress { get; set; }

        /// <summary>
        /// 活动标题
        /// </summary>
        public string ActivityTitle { get; set; }

        /// <summary>
        /// 活动标题 英文
        /// </summary>
        public string EActivityTitle { get; set; }

        /// <summary>
        /// 活动地址 英文
        /// </summary>
        public string EActivityAddress { get; set; }

        /// <summary>
        /// 预约车型
        /// </summary>
        public string CarName { get; set; }
        /// <summary>
        /// 经销商名称
        /// </summary>
        public string StoreName { get; set; }
        /// <summary>
        /// 经销商地址
        /// </summary>
        public string StoreAddress { get; set; }
        /// <summary>
        /// 经销商电话
        /// </summary>
        public string StorePhone { get; set; }

        /// <summary>
        /// 经销商名称(英文)
        /// </summary>
        public string EStoreName { get; set; }
        /// <summary>
        /// 经销商地址(英文)
        /// </summary>
        public string EStoreAddress { get; set; }
        /// <summary>
        /// 经销商电话(英文)
        /// </summary>
        public string EStorePhone { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// 类型参数
        /// </summary>
        public string Option { get; set; }
        /// <summary>
        /// 预约类型
        /// </summary>
        public string AppointmentType { get; set; }

        /// <summary>
        /// 预约服务类型 0维修 1其他
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 是否上门服务
        /// </summary>
        public bool IsDoor { get; set; }
    }

    public class AppointmentEditDto : EntityDto
    {
        /// <summary>
        /// 性别 0男 1女
        /// </summary>
        public int Sex { get; set; }
        /// <summary>
        /// 门店Id
        /// </summary>
        [Required]
        public int StoreId { get; set; }


        /// <summary>
        /// 预约我的车辆
        /// </summary>
        [Required]
        public int UserCarId { get; set; }

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
        /// 预约服务日期
        /// </summary>
        [Required]
        public DateTime Date { get; set; }

        /// <summary>
        /// 预约服务类型 0维修 1其他
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 是否上门服务
        /// </summary>
        public bool IsDoor { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// 地区
        /// </summary>
        public string Area { get; set; }

        /// <summary>
        /// 街道
        /// </summary>
        public string Street { get; set; }

        /// <summary>
        /// 详细地址
        /// </summary>
        public string DetailAddress { get; set; }
    }

    public class AdminAppointmentEditDto : EntityDto
    {

        /// <summary>
        /// 状态 0未到店/1已到店
        /// </summary>
        [Required]
        public int State { get; set; }

        /// <summary>
        /// 联系状态 0未联系/1联系失败/2延迟提醒/3确认预约
        /// </summary>
        public int ContactState { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; }
    }
    public class AppointmentDto : EntityDto
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 性别 0男 1女
        /// </summary>
        public int Sex { get; set; }

        /// <summary>
        /// 门店Id
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// 预约我的车辆
        /// </summary>
        public int UserCarId { get; set; }

        /// <summary>
        /// 车架号
        /// </summary>
        public string FrameNo { get; set; }
        /// <summary>
        /// 预约车型
        /// </summary>
        public int CarId { get; set; }

        /// <summary>
        /// 姓
        /// </summary>
        public string SurName { get; set; }

        /// <summary>
        /// 名
        /// </summary>
        public string Name { get; set; }

        public string FullName { get { return (this.SurName + " " + this.Name); } }

        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 预约服务日期
        /// </summary>
        public DateTime? Date { get; set; }
        /// <summary>
        /// 预约车型
        /// </summary>
        public string CarName { get; set; }
        /// <summary>
        /// 经销商名称
        /// </summary>
        public string StoreName { get; set; }
        /// <summary>
        /// 经销商地址
        /// </summary>
        public string StoreAddress { get; set; }
        /// <summary>
        /// 经销商电话
        /// </summary>
        public string StorePhone { get; set; }

        /// <summary>
        /// 经销商名称(英文)
        /// </summary>
        public string EStoreName { get; set; }
        /// <summary>
        /// 经销商地址(英文)
        /// </summary>
        public string EStoreAddress { get; set; }
        /// <summary>
        /// 经销商电话(英文)
        /// </summary>
        public string EStorePhone { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }
        /// <summary>
        /// 城市
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// 地区
        /// </summary>
        public string Area { get; set; }

        /// <summary>
        /// 街道
        /// </summary>
        public string Street { get; set; }

        /// <summary>
        /// 详细地址
        /// </summary>
        public string DetailAddress { get; set; }

        /// <summary>
        /// 预约类型
        /// </summary>
        public string AppointmentType { get { return "Service"; } }

        /// <summary>
        /// 状态 0未到店/1已到店
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 联系状态 0未联系/1联系失败/2延迟提醒/3确认预约
        /// </summary>
        public int ContactState { get; set; }

        public string StateText { get { return this.State == 0 ? "未到店" : "已到店"; } }

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

        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// 预约服务类型 0维修 1其他
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 是否上门服务
        /// </summary>
        public bool IsDoor { get; set; }
    }
}
