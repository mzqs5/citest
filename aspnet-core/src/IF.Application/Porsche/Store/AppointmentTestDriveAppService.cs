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

        #region ��ȡԤԼ�Լݵ������Ϣ��֧�ַ�ҳ��ѯ
        /// <summary>
        /// ��ȡԤԼ�Լݵ������Ϣ��֧�ַ�ҳ��ѯ
        /// </summary>
        /// <param name="loadOptions">��ѯ����</param>
        /// <returns></returns>
        public async Task<object> GetListDataAsync(DataSourceLoadOptions loadOptions)
        {
            loadOptions.Sort = new SortingInfo[] { new SortingInfo() { Selector = "CreationTime", Desc = true } };
            return base.DataSourceLoadMap(GetDataSource(), loadOptions);
        }
        #endregion

        #region ����
        [HttpGet]
        //[AbpAuthorize("Admin.AppointmentTestDrive.Export")]
        public async Task<IActionResult> Export(DataSourceLoadOptions loadOptions)
        {
            loadOptions.Sort = new SortingInfo[] { new SortingInfo() { Selector = "CreationTime", Desc = true } };
            var Export = new ExportHelper();
            Export.AddColumn("Surname", "��");
            Export.AddColumn("Name", "��");
            Export.AddColumn("Mobile", "�ֻ�����");
            Export.AddColumn("Date", "ԤԼ����");
            Export.AddColumn("Sex", "�Ա�");
            Export.AddColumn("CarName", "����");
            Export.AddColumn("FrameNo", "���ܺ�");
            Export.AddColumn("StoreName", "�ŵ�����");
            Export.AddColumn("StateText", "״̬");
            Export.AddColumn("ContactStateText", "��ϵ״̬");
            Export.AddColumn("Remarks", "��ע");
            var result1 = Export.ExportExcel<AppointmentTestDriveDto>(base.DataSourceLoadMap(GetDataSource(), loadOptions).data);
            result1.FileDownloadName = "ԤԼ�Լݼ�¼";
            return result1;
        }

        #endregion

        #region ��ȡ�ҵ�ԤԼ�Լݵ������Ϣ��֧�ַ�ҳ��ѯ
        /// <summary>
        /// ��ȡ�ҵ�ԤԼ�Լݵ������Ϣ��֧�ַ�ҳ��ѯ
        /// </summary>
        /// <param name="loadOptions">��ѯ����</param>
        /// <returns></returns>
        public async Task<object> GetMyListDataAsync(DataSourceLoadOptions loadOptions)
        {
            loadOptions.Sort = new SortingInfo[] { new SortingInfo() { Selector = "CreationTime", Desc = true } };
            return base.DataSourceLoadMap(GetDataSource().Where(p => p.UserId == AbpSession.UserId.Value), loadOptions);
        }
        #endregion

        #region ����ID��ȡԤԼ�Լ���Ϣ
        /// <summary>
        /// ����ID��ȡԤԼ�Լ���Ϣ
        /// </summary>
        /// <param name="id">ԤԼ�Լ���Ϣ����</param>
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
                Logger.Error("ԤԼ�Լݻ�ȡ�쳣��", e);
                throw new AbpException("ԤԼ�Լݻ�ȡ�쳣", e);
            }
        }
        #endregion

        #region  �������߸���ԤԼ�Լ���Ϣ
        /// <summary>
        /// �������߸���ԤԼ�Լ���Ϣ
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
                Logger.Error("ԤԼ�Լݱ����쳣��", e);
                throw new AbpException("ԤԼ�Լݱ����쳣��", e);
            }
        }
        #endregion

        #region  �޸�ԤԼ�Լ���Ϣ �����̨ʹ��
        /// <summary>
        /// �޸�ԤԼ�Լ���Ϣ �����̨ʹ��
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
                Logger.Error("ԤԼ�Լݱ����쳣��", e);
                throw new AbpException("ԤԼ�Լݱ����쳣��", e);
            }
        }
        #endregion

        #region ����ԤԼ�Լݵ�Ĭ�ϲ���
        /// <summary>
        /// ����ԤԼ�Լݵ�Ĭ�ϲ���
        /// </summary>
        /// <returns></returns>
        public async Task<AppointmentTestDriveEditDto> GetEmptyEntityAsync()
        {
            return await Task.Run(() => new AppointmentTestDriveEditDto());
        }
        #endregion

        #region ����ɾ��ԤԼ�Լ�
        /// <summary>
        /// ����ɾ��ԤԼ�Լ�
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
                Logger.Error("ԤԼ�Լ�ɾ���쳣��", e);
                throw new AbpException("ԤԼ�Լ�ɾ���쳣��", e);
            }
        }
        #endregion

        #region ����
        /// <summary>
        /// ����
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
            //var list = AppointmentTestDriveRepository.GetAll().WhereIf(id != 0, p => p.IsDeleted == false && p.StoreId == id).ToList();
            var list = AppointmentTestDriveRepository.GetAll().AsEnumerable();
            if (id != 0)
                list = list.Where(p => p.IsDeleted == false && p.StoreId == id);
            return new {
                n = "�Լ�",
                n1 = "ԤԼ��",
                n2 = "������",
                n3 = "δ������",
                t1 = list.Count(), t2 = list.Where(p => p.State == 1).Count(), t3 = list.Where(p => p.State == 0).Count() };
        }
        #endregion
    }

    public class AppointmentTestDriveEditDto : EntityDto
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
        /// ԤԼ����
        /// </summary>
        [Required]
        public int CarId { get; set; }
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
        public string Mobile { get; set; }


        /// <summary>
        /// ԤԼ�Լ�����
        /// </summary>
        //[Required]
        public DateTime? Date { get; set; }

    }

    public class AdminAppointmentTestDriveEditDto : EntityDto
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
    public class AppointmentTestDriveDto : EntityDto
    {
        /// <summary>
        /// �Ա� 0�� 1Ů
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
        /// ԤԼ�Լ�����
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
        /// ԤԼ����
        /// </summary>
        public string AppointmentType { get { return "TestDrive"; } }

        /// <summary>
        /// ����ʱ��
        /// </summary>
        public DateTime CreationTime { get; set; }

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
    }
}
