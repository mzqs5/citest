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
    public class AppointmentTestDriveAppService : IFAppServiceBase, IAppointmentTestDriveAppService
    {
        IRepository<AppointmentTestDriveAggregate> AppointmentTestDriveRepository;
        IRepository<StoreAggregate> StoreRepository;
        IRepository<User,long> UserRepository;
        IRepository<CarAggregate> CarRepository;
        public AppointmentTestDriveAppService(
            IRepository<AppointmentTestDriveAggregate> AppointmentTestDriveRepository,
            IRepository<StoreAggregate> StoreRepository,
            IRepository<CarAggregate> CarRepository,
            IRepository<User, long> UserRepository)
        {
            this.AppointmentTestDriveRepository = AppointmentTestDriveRepository;
            this.StoreRepository = StoreRepository;
            this.CarRepository = CarRepository;
            this.UserRepository = UserRepository;
        }

        private IEnumerable<AppointmentTestDriveDto> GetDataSource()
        {
            var result = from Appointment in AppointmentTestDriveRepository.GetAll().AsEnumerable()
                         join Car in CarRepository.GetAll().AsEnumerable()
                         on Appointment.CarId equals Car.Id
                         join Store in StoreRepository.GetAll().AsEnumerable()
                         on Appointment.StoreId equals Store.Id
                         into Stores
                         from Store in Stores.DefaultIfEmpty()
                         select new AppointmentTestDriveDto
                         {
                             Id = Appointment.Id,
                             Name = Appointment.Name,
                             Mobile = Appointment.Mobile,
                             CarId = Appointment.CarId,
                             CarName = Car.Name,
                             StoreId = Appointment.StoreId,
                             StoreName = Store == null ? "" : Store.Name,
                             StoreAddress = Store == null ? "" : Store.Address,
                             StorePhone = Store == null ? "" : Store.Phone,
                             EStoreName = Store == null ? "" : Store.EName,
                             EStoreAddress = Store == null ? "" : Store.EAddress,
                             EStorePhone = Store == null ? "" : Store.EPhone,
                             SurName = Appointment.SurName,
                             UserId = Appointment.UserId,
                             Sex = Appointment.Sex,
                             Date = Appointment.Date,
                             CreationTime = Appointment.CreationTime,
                             State = Appointment.State,
                             ContactState = Appointment.ContactState,
                             Remarks = Appointment.Remarks
                         };
            return result;
        }

        #region 获取预约试驾的相关信息，支持分页查询
        /// <summary>
        /// 获取预约试驾的相关信息，支持分页查询
        /// </summary>
        /// <param name="loadOptions">查询参数</param>
        /// <returns></returns>
        public async Task<object> GetListDataAsync(DataSourceLoadOptions loadOptions)
        {
            loadOptions.Sort = new SortingInfo[] { new SortingInfo() { Selector = "CreationTime", Desc = true } };
            return base.DataSourceLoadMap(GetDataSource(), loadOptions);
        }
        #endregion

        #region 导出
        [HttpGet]
        //[AbpAuthorize("Admin.AppointmentTestDrive.Export")]
        public async Task<IActionResult> Export(DataSourceLoadOptions loadOptions)
        {
            loadOptions.Sort = new SortingInfo[] { new SortingInfo() { Selector = "CreationTime", Desc = true } };
            var Export = new ExportHelper();
            Export.AddColumn("Surname", "姓");
            Export.AddColumn("Name", "名");
            Export.AddColumn("Mobile", "手机号码");
            Export.AddColumn("Date", "预约日期");
            Export.AddColumn("Sex", "性别");
            Export.AddColumn("CarName", "车型");
            Export.AddColumn("FrameNo", "车架号");
            Export.AddColumn("StoreName", "门店名称");
            Export.AddColumn("StateText", "状态");
            Export.AddColumn("ContactStateText", "联系状态");
            Export.AddColumn("Remarks", "备注");
            var result1 = Export.ExportExcel<AppointmentTestDriveDto>(base.DataSourceLoadMap(GetDataSource(), loadOptions).data);
            result1.FileDownloadName = "预约试驾记录";
            return result1;
        }

        #endregion

        #region 获取我的预约试驾的相关信息，支持分页查询
        /// <summary>
        /// 获取我的预约试驾的相关信息，支持分页查询
        /// </summary>
        /// <param name="loadOptions">查询参数</param>
        /// <returns></returns>
        public async Task<object> GetMyListDataAsync(DataSourceLoadOptions loadOptions)
        {
            loadOptions.Sort = new SortingInfo[] { new SortingInfo() { Selector = "CreationTime", Desc = true } };
            return base.DataSourceLoadMap(GetDataSource().Where(p => p.UserId == AbpSession.UserId.Value), loadOptions);
        }
        #endregion

        #region 根据ID获取预约试驾信息
        /// <summary>
        /// 根据ID获取预约试驾信息
        /// </summary>
        /// <param name="id">预约试驾信息主键</param>
        /// <returns></returns>
        public async Task<AppointmentTestDriveDto> GetAsync(int id)
        {
            try
            {
                var AppointmentTestDrive = await AppointmentTestDriveRepository.GetAll().Where(k => k.Id == id).FirstOrDefaultAsync();

                if (AppointmentTestDrive == null)
                {
                    throw new EntityNotFoundException(typeof(AppointmentTestDriveAggregate), id);
                }
                var dto = this.ObjectMapper.Map<AppointmentTestDriveDto>(AppointmentTestDrive);
                var Car = await CarRepository.FirstOrDefaultAsync(p => p.Id == AppointmentTestDrive.CarId);
                if (Car != null)
                {
                    dto.CarName = Car.Name;
                }
                var Store = await StoreRepository.FirstOrDefaultAsync(p => p.Id == AppointmentTestDrive.StoreId);
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
                Logger.Error("预约试驾获取异常！", e);
                throw new AbpException("预约试驾获取异常", e);
            }
        }
        #endregion

        #region  新增或者更改预约试驾信息
        /// <summary>
        /// 新增或者更改预约试驾信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task SaveAsync(AppointmentTestDriveEditDto input)
        {
            input.StoreId = ExportHelper.GetStoreId(input.StoreId);
            try
            {
                AppointmentTestDriveAggregate entity = this.ObjectMapper.Map<AppointmentTestDriveAggregate>(input);
                entity.UserId = AbpSession.UserId.Value;
                var user = await UserRepository.FirstOrDefaultAsync(p => p.Id == AbpSession.UserId.Value);
                user.LastStoreId = input.StoreId;
                user.Name = input.Name;
                user.Surname = input.SurName;
                await UserRepository.UpdateAsync(user);
                if (input.Id != 0)
                {
                    AppointmentTestDriveAggregate AppointmentTestDriveAggregate = AppointmentTestDriveRepository.GetAll().Where(k => k.Id == input.Id).FirstOrDefault();
                    AppointmentTestDriveAggregate.Name = entity.Name;
                    AppointmentTestDriveAggregate.SurName = entity.SurName;
                    AppointmentTestDriveAggregate.StoreId = entity.StoreId;
                    AppointmentTestDriveAggregate.Sex = entity.Sex;
                    AppointmentTestDriveAggregate.CarId = entity.CarId;
                    AppointmentTestDriveAggregate.Mobile = entity.Mobile;
                    AppointmentTestDriveAggregate.Date = entity.Date;

                    await AppointmentTestDriveRepository.UpdateAsync(AppointmentTestDriveAggregate);
                }
                else
                {
                    await AppointmentTestDriveRepository.InsertAsync(entity);
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

        #region  修改预约试驾信息 管理后台使用
        /// <summary>
        /// 修改预约试驾信息 管理后台使用
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [AbpAuthorize("Admin.AppointmentTestDrive.Edit")]
        public async Task AdminSaveAsync(AdminAppointmentTestDriveEditDto input)
        {
            try
            {
                if (input.Id != 0)
                {
                    AppointmentTestDriveAggregate AppointmentTestDriveAggregate = AppointmentTestDriveRepository.FirstOrDefault(k => k.Id == input.Id);
                    AppointmentTestDriveAggregate.State = input.State;
                    AppointmentTestDriveAggregate.ContactState = input.ContactState;
                    AppointmentTestDriveAggregate.Remarks = input.Remarks;

                    await AppointmentTestDriveRepository.UpdateAsync(AppointmentTestDriveAggregate);
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

        #region 新增预约试驾的默认参数
        /// <summary>
        /// 新增预约试驾的默认参数
        /// </summary>
        /// <returns></returns>
        public async Task<AppointmentTestDriveEditDto> GetEmptyEntityAsync()
        {
            return await Task.Run(() => new AppointmentTestDriveEditDto());
        }
        #endregion

        #region 批量删除预约试驾
        /// <summary>
        /// 批量删除预约试驾
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete]
        [AbpAuthorize("Admin.AppointmentTestDrive.Edit")]
        public async Task BatchDeleteAsync(string ids)
        {
            try
            {
                foreach (var id in ids.Split(','))
                {
                    await AppointmentTestDriveRepository.DeleteAsync(kv => kv.Id == int.Parse(id));
                }
                await CurrentUnitOfWork.SaveChangesAsync();

            }
            catch (Exception e)
            {
                Logger.Error("预约试驾删除异常！", e);
                throw new AbpException("预约试驾删除异常！", e);
            }
        }
        #endregion

        #region 核销
        /// <summary>
        /// 核销
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AbpAuthorize("Admin.AppointmentTestDrive.WriteOff")]
        public async Task WriteOff(string ids)
        {
            try
            {
                foreach (var id in ids.Split(','))
                {
                    var entity = await AppointmentTestDriveRepository.FirstOrDefaultAsync(p => p.Id == int.Parse(id));
                    entity.State = 1;
                    await AppointmentTestDriveRepository.UpdateAsync(entity);
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
            //var list = AppointmentTestDriveRepository.GetAll().WhereIf(id != 0, p => p.IsDeleted == false && p.StoreId == id).ToList();
            var list = AppointmentTestDriveRepository.GetAll().AsEnumerable();
            if (id != 0)
                list = list.Where(p => p.IsDeleted == false && p.StoreId == id);
            return new {
                n = "试驾",
                n1 = "预约数",
                n2 = "到店数",
                n3 = "未到店数",
                t1 = list.Count(), t2 = list.Where(p => p.State == 1).Count(), t3 = list.Where(p => p.State == 0).Count() };
        }
        #endregion
    }

    public class AppointmentTestDriveEditDto : EntityDto
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
        /// 预约车型
        /// </summary>
        [Required]
        public int CarId { get; set; }
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
        public string Mobile { get; set; }


        /// <summary>
        /// 预约试驾日期
        /// </summary>
        //[Required]
        public DateTime? Date { get; set; }

    }

    public class AdminAppointmentTestDriveEditDto : EntityDto
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
    public class AppointmentTestDriveDto : EntityDto
    {
        /// <summary>
        /// 性别 0男 1女
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
        /// 预约试驾日期
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
        /// 预约类型
        /// </summary>
        public string AppointmentType { get { return "TestDrive"; } }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; }

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
    }
}
