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

        #region ��ȡԤԼ����������Ϣ��֧�ַ�ҳ��ѯ
        /// <summary>
        /// ��ȡԤԼ����������Ϣ��֧�ַ�ҳ��ѯ
        /// </summary>
        /// <param name="loadOptions">��ѯ����</param>
        /// <returns></returns>
        public async Task<object> GetListDataAsync(DataSourceLoadOptions loadOptions)
        {
            loadOptions.Sort = new SortingInfo[] { new SortingInfo() { Selector = "CreationTime", Desc = true } };
            return base.DataSourceLoadMap(GetDataSource(), loadOptions);
        }
        #endregion

        #region ��ȡ�ҵ�ԤԼ����������Ϣ��֧�ַ�ҳ��ѯ
        /// <summary>
        /// ��ȡ�ҵ�ԤԼ����������Ϣ��֧�ַ�ҳ��ѯ
        /// </summary>
        /// <param name="loadOptions">��ѯ����</param>
        /// <returns></returns>
        public async Task<object> GetMyListDataAsync(DataSourceLoadOptions loadOptions)
        {
            loadOptions.Sort = new SortingInfo[] { new SortingInfo() { Selector = "CreationTime", Desc = true } };
            return base.DataSourceLoadMap(GetDataSource().Where(p => p.UserId == AbpSession.UserId.Value), loadOptions);
        }
        #endregion

        #region ��ȡ�ҵ�����ԤԼ��֧�ַ�ҳ��ѯ

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
        /// ��ȡ�ҵ�����ԤԼ��֧�ַ�ҳ��ѯ
        /// </summary>
        /// <param name="loadOptions">��ѯ����</param>
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

        #region ����ID��ȡԤԼ������Ϣ
        /// <summary>
        /// ����ID��ȡԤԼ������Ϣ
        /// </summary>
        /// <param name="id">ԤԼ������Ϣ����</param>
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
                Logger.Error("ԤԼ�����ȡ�쳣��", e);
                throw new AbpException("ԤԼ�����ȡ�쳣", e);
            }
        }
        #endregion

        #region  �������߸���ԤԼ������Ϣ
        /// <summary>
        /// �������߸���ԤԼ������Ϣ
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
                throw new AbpException("�Ƿ�����id");
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

        #region  �޸�ԤԼ������Ϣ �����̨ʹ��
        /// <summary>
        /// �޸�ԤԼ������Ϣ �����̨ʹ��
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
                Logger.Error("ԤԼ�Լݱ����쳣��", e);
                throw new AbpException("ԤԼ�Լݱ����쳣��", e);
            }
        }
        #endregion

        #region ����ԤԼ�����Ĭ�ϲ���
        /// <summary>
        /// ����ԤԼ�����Ĭ�ϲ���
        /// </summary>
        /// <returns></returns>
        public async Task<AppointmentEditDto> GetEmptyEntityAsync()
        {
            return await Task.Run(() => new AppointmentEditDto());
        }
        #endregion

        #region ����ɾ��ԤԼ����
        /// <summary>
        /// ����ɾ��ԤԼ����
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
                Logger.Error("ԤԼ����ɾ���쳣��", e);
                throw new AbpException("ԤԼ����ɾ���쳣��", e);
            }
        }
        #endregion

        #region ����
        /// <summary>
        /// ����
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
                Logger.Error("ԤԼ�Լݺ����쳣��", e);
                throw new AbpException("ԤԼ�Լݺ����쳣��", e);
            }
        }
        #endregion

        #region ��ȡͳ��
        /// <summary>
        /// ��ȡͳ��
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
                n = "����",
                n1 = "ԤԼ��",
                n2 = "�����",
                n3 = "δ�����",
                t1 = list.Count(), t2 = list.Where(p => p.State == 1).Count(), t3 = list.Where(p => p.State == 0).Count() };
        }
        #endregion
    }

    internal class AllAppointmentDto : EntityDto
    {
        /// <summary>
        /// �Ա�
        /// </summary>
        public int Sex { get; set; }
        /// <summary>
        /// �û�Id
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// �ŵ�Id
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// ԤԼ�ҵĳ���
        /// </summary>
        public int UserCarId { get; set; }

        /// <summary>
        /// ���ܺ�
        /// </summary>
        public string FrameNo { get; set; }

        /// <summary>
        /// ԤԼ����
        /// </summary>
        public int CarId { get; set; }

        /// <summary>
        /// ��
        /// </summary>
        public string SurName { get; set; }

        /// <summary>
        /// ��
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// �ֻ���
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// ԤԼ��������
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// ���ַ
        /// </summary>
        public string ActivityAddress { get; set; }

        /// <summary>
        /// �����
        /// </summary>
        public string ActivityTitle { get; set; }

        /// <summary>
        /// ����� Ӣ��
        /// </summary>
        public string EActivityTitle { get; set; }

        /// <summary>
        /// ���ַ Ӣ��
        /// </summary>
        public string EActivityAddress { get; set; }

        /// <summary>
        /// ԤԼ����
        /// </summary>
        public string CarName { get; set; }
        /// <summary>
        /// ����������
        /// </summary>
        public string StoreName { get; set; }
        /// <summary>
        /// �����̵�ַ
        /// </summary>
        public string StoreAddress { get; set; }
        /// <summary>
        /// �����̵绰
        /// </summary>
        public string StorePhone { get; set; }

        /// <summary>
        /// ����������(Ӣ��)
        /// </summary>
        public string EStoreName { get; set; }
        /// <summary>
        /// �����̵�ַ(Ӣ��)
        /// </summary>
        public string EStoreAddress { get; set; }
        /// <summary>
        /// �����̵绰(Ӣ��)
        /// </summary>
        public string EStorePhone { get; set; }

        /// <summary>
        /// ����ʱ��
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// ���Ͳ���
        /// </summary>
        public string Option { get; set; }
        /// <summary>
        /// ԤԼ����
        /// </summary>
        public string AppointmentType { get; set; }

        /// <summary>
        /// ԤԼ�������� 0ά�� 1����
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// �Ƿ����ŷ���
        /// </summary>
        public bool IsDoor { get; set; }
    }

    public class AppointmentEditDto : EntityDto
    {
        /// <summary>
        /// �Ա� 0�� 1Ů
        /// </summary>
        public int Sex { get; set; }
        /// <summary>
        /// �ŵ�Id
        /// </summary>
        [Required]
        public int StoreId { get; set; }


        /// <summary>
        /// ԤԼ�ҵĳ���
        /// </summary>
        [Required]
        public int UserCarId { get; set; }

        /// <summary>
        /// ��
        /// </summary>
        [Required]
        [StringLength(20)]
        public string SurName { get; set; }

        /// <summary>
        /// ��
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Name { get; set; }


        /// <summary>
        /// �ֻ���
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Mobile { get; set; }


        /// <summary>
        /// ԤԼ��������
        /// </summary>
        [Required]
        public DateTime Date { get; set; }

        /// <summary>
        /// ԤԼ�������� 0ά�� 1����
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// �Ƿ����ŷ���
        /// </summary>
        public bool IsDoor { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string Area { get; set; }

        /// <summary>
        /// �ֵ�
        /// </summary>
        public string Street { get; set; }

        /// <summary>
        /// ��ϸ��ַ
        /// </summary>
        public string DetailAddress { get; set; }
    }

    public class AdminAppointmentEditDto : EntityDto
    {

        /// <summary>
        /// ״̬ 0δ����/1�ѵ���
        /// </summary>
        [Required]
        public int State { get; set; }

        /// <summary>
        /// ��ϵ״̬ 0δ��ϵ/1��ϵʧ��/2�ӳ�����/3ȷ��ԤԼ
        /// </summary>
        public int ContactState { get; set; }

        /// <summary>
        /// ��ע
        /// </summary>
        public string Remarks { get; set; }
    }
    public class AppointmentDto : EntityDto
    {
        /// <summary>
        /// �û�Id
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// �Ա� 0�� 1Ů
        /// </summary>
        public int Sex { get; set; }

        /// <summary>
        /// �ŵ�Id
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// ԤԼ�ҵĳ���
        /// </summary>
        public int UserCarId { get; set; }

        /// <summary>
        /// ���ܺ�
        /// </summary>
        public string FrameNo { get; set; }
        /// <summary>
        /// ԤԼ����
        /// </summary>
        public int CarId { get; set; }

        /// <summary>
        /// ��
        /// </summary>
        public string SurName { get; set; }

        /// <summary>
        /// ��
        /// </summary>
        public string Name { get; set; }

        public string FullName { get { return (this.SurName + " " + this.Name); } }

        /// <summary>
        /// �ֻ���
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// ԤԼ��������
        /// </summary>
        public DateTime? Date { get; set; }
        /// <summary>
        /// ԤԼ����
        /// </summary>
        public string CarName { get; set; }
        /// <summary>
        /// ����������
        /// </summary>
        public string StoreName { get; set; }
        /// <summary>
        /// �����̵�ַ
        /// </summary>
        public string StoreAddress { get; set; }
        /// <summary>
        /// �����̵绰
        /// </summary>
        public string StorePhone { get; set; }

        /// <summary>
        /// ����������(Ӣ��)
        /// </summary>
        public string EStoreName { get; set; }
        /// <summary>
        /// �����̵�ַ(Ӣ��)
        /// </summary>
        public string EStoreAddress { get; set; }
        /// <summary>
        /// �����̵绰(Ӣ��)
        /// </summary>
        public string EStorePhone { get; set; }
        /// <summary>
        /// ����ʱ��
        /// </summary>
        public DateTime CreationTime { get; set; }
        /// <summary>
        /// ����
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string Area { get; set; }

        /// <summary>
        /// �ֵ�
        /// </summary>
        public string Street { get; set; }

        /// <summary>
        /// ��ϸ��ַ
        /// </summary>
        public string DetailAddress { get; set; }

        /// <summary>
        /// ԤԼ����
        /// </summary>
        public string AppointmentType { get { return "Service"; } }

        /// <summary>
        /// ״̬ 0δ����/1�ѵ���
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// ��ϵ״̬ 0δ��ϵ/1��ϵʧ��/2�ӳ�����/3ȷ��ԤԼ
        /// </summary>
        public int ContactState { get; set; }

        public string StateText { get { return this.State == 0 ? "δ����" : "�ѵ���"; } }

        public string ContactStateText
        {
            get
            {
                switch (this.ContactState)
                {
                    case 1:
                        return "��ϵʧ��";
                    case 2:
                        return "�ӳ�����";
                    case 3:
                        return "ȷ��ԤԼ";
                    default:
                        return "δ��ϵ";
                }
            }
        }

        /// <summary>
        /// ��ע
        /// </summary>
        public string Remarks { get; set; }

        /// <summary>
        /// ԤԼ�������� 0ά�� 1����
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// �Ƿ����ŷ���
        /// </summary>
        public bool IsDoor { get; set; }
    }
}
