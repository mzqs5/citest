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
        ///  ��ȡ�������ѵ������Ϣ��֧�ַ�ҳ��ѯ
        /// </summary>
        /// <param name="loadOptions">��ѯ����</param>
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
        ///  ��ȡ�������ѵ������Ϣ��֧�ַ�ҳ��ѯ
        /// </summary>
        /// <param name="loadOptions">��ѯ����</param>
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

        #region ����ID��ȡ����������Ϣ
        /// <summary>
        /// ����ID��ȡ����������Ϣ
        /// </summary>
        /// <param name="id">����������Ϣ����</param>
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
                Logger.Error("�������ѻ�ȡ�쳣��", e);
                throw new AbpException("�������ѻ�ȡ�쳣", e);
            }
        }
        #endregion

        #region  �������߸��Ķ���������Ϣ
        /// <summary>
        /// �������߸��Ķ���������Ϣ
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
                Logger.Error("�������ѱ����쳣��", e);
                throw new AbpException("�������ѱ����쳣��", e);
            }
        }
        #endregion

        #region �����������ѵ�Ĭ�ϲ���
        /// <summary>
        /// �����������ѵ�Ĭ�ϲ���
        /// </summary>
        /// <returns></returns>
        public async Task<AppointmentSmsEditDto> GetEmptyEntityAsync()
        {
            return await Task.Run(() => new AppointmentSmsEditDto());
        }
        #endregion

        #region ����ɾ����������
        /// <summary>
        /// ����ɾ����������
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
                Logger.Error("��������ɾ���쳣��", e);
                throw new AbpException("��������ɾ���쳣��", e);
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
        /// �Ƿ��Ѷ�
        /// </summary>
        public bool IsAlreadyRead { get; set; }
    }

    public class AppointmentSmsDto : EntityDto
    {
        /// <summary>
        /// �û�Id
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// �û��ֻ���
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// �û�����
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// �Ƿ��Ѷ�
        /// </summary>
        public bool IsAlreadyRead { get; set; }

        /// <summary>
        /// ״̬
        /// </summary>
        public string StateText { get { return this.IsAlreadyRead ? "�Ѷ�" : "δ��"; } }

        /// <summary>
        /// ������Ϣ
        /// </summary>
        public string Msg { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
