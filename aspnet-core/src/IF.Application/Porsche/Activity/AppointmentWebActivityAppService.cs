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

        #region ��ȡ��վ�ԤԼ�������Ϣ��֧�ַ�ҳ��ѯ
        /// <summary>
        /// ��ȡ��վ�ԤԼ�������Ϣ��֧�ַ�ҳ��ѯ
        /// </summary>
        /// <param name="loadOptions">��ѯ����</param>
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

        #region ��ȡ�ҵ���վ�ԤԼ�������Ϣ��֧�ַ�ҳ��ѯ
        /// <summary>
        /// ��ȡ�ҵ���վ�ԤԼ�������Ϣ��֧�ַ�ҳ��ѯ
        /// </summary>
        /// <param name="loadOptions">��ѯ����</param>
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

        #region ����ID��ȡ��վ�ԤԼ��Ϣ
        /// <summary>
        /// ����ID��ȡ��վ�ԤԼ��Ϣ
        /// </summary>
        /// <param name="id">��վ�ԤԼ��Ϣ����</param>
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
                Logger.Error("��վ�ԤԼ��ȡ�쳣��", e);
                throw new AbpException("��վ�ԤԼ��ȡ�쳣", e);
            }
        }
        #endregion

        #region  �������߸�����վ�ԤԼ��Ϣ
        /// <summary>
        /// �������߸�����վ�ԤԼ��Ϣ
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
                Logger.Error("��վ�ԤԼ�����쳣��", e);
                throw new AbpException("��վ�ԤԼ�����쳣��", e);
            }
        }
        #endregion

        #region  ������վ�ԤԼ��Ϣ
        /// <summary>
        /// ������վ�ԤԼ��Ϣ
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
                Logger.Error("ԤԼ������쳣��", e);
                throw new AbpException("ԤԼ������쳣��", e);
            }
        }
        #endregion

        #region ������վ�ԤԼ��Ĭ�ϲ���
        /// <summary>
        /// ������վ�ԤԼ��Ĭ�ϲ���
        /// </summary>
        /// <returns></returns>
        public async Task<AppointmentWebActivityEditDto> GetEmptyEntityAsync()
        {
            return await Task.Run(() => new AppointmentWebActivityEditDto());
        }
        #endregion

        #region ����ɾ����վ�ԤԼ
        /// <summary>
        /// ����ɾ����վ�ԤԼ
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
                Logger.Error("��վ�ԤԼɾ���쳣��", e);
                throw new AbpException("��վ�ԤԼɾ���쳣��", e);
            }
        }
        #endregion

        #region ����
        /// <summary>
        /// ����
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
                Logger.Error("ԤԼ������쳣��", e);
                throw new AbpException("ԤԼ������쳣��", e);
            }
        }
        #endregion
    }

    public class AppointmentWebActivityEditDto : EntityDto
    {
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
        /// �Id
        /// </summary>
        [Required]
        public int ActivityId { get; set; }


        /// <summary>
        /// �ֻ���
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Mobile { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        [StringLength(50)]
        public string Option { get; set; }
    }


    public class AdminAppointmentWebActivityEditDto : EntityDto
    {
        /// <summary>
        /// ״̬ 0��Ч/1����/2��Ч
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// ��ϵ״̬ 0δ��ϵ/1��ϵʧ��/2�ӳ�����/3ȷ��ԤԼ
        /// </summary>
        public int ContactState { get; set; }

        public string Remarks { get; set; }
    }
    public class AppointmentWebActivityDto : EntityDto
    {

        /// <summary>
        /// �Id
        /// </summary>
        public int ActivityId { get; set; }

        /// <summary>
        /// �����
        /// </summary>
        public int ActivityType { get; set; }

        /// <summary>
        /// �����
        /// </summary>
        public string ActivityTitle { get; set; }

        /// <summary>
        /// ���ַ
        /// </summary>
        public string ActivityAddress { get; set; }

        /// <summary>
        /// ����� Ӣ��
        /// </summary>
        public string EActivityTitle { get; set; }

        /// <summary>
        /// ���ַ Ӣ��
        /// </summary>
        public string EActivityAddress { get; set; }

        /// <summary>
        /// �û�Id
        /// </summary>
        public long UserId { get; set; }

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
        /// ����
        /// </summary>
        public string Option { get; set; }

        /// <summary>
        /// ����ʱ��
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// ״̬ 0��Ч/1����/2��Ч
        /// </summary>
        public int State { get; set; }

        public string StateText
        {
            get
            {
                switch (this.State)
                {
                    case 1: return "����";
                    case 2: return "��Ч";
                    default: return "��Ч";
                }
            }
        }
        /// <summary>
        /// ԤԼ����
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
        /// ��ϵ״̬ 0δ��ϵ/1��ϵʧ��/2�ӳ�����/3ȷ��ԤԼ
        /// </summary>
        public int ContactState { get; set; }

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

        public string Remarks { get; set; }
    }
}
