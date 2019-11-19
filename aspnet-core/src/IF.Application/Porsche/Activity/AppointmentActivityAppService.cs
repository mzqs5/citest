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

        #region ��ȡԤԼ��������Ϣ��֧�ַ�ҳ��ѯ
        /// <summary>
        /// ��ȡԤԼ��������Ϣ��֧�ַ�ҳ��ѯ
        /// </summary>
        /// <param name="loadOptions">��ѯ����</param>
        /// <returns></returns>
        public async Task<object> GetListDataAsync(DataSourceLoadOptions loadOptions)
        {
            return base.DataSourceLoadMap(GetDataSource(), loadOptions);
        }
        #endregion

        #region ��ȡ�ҵ�ԤԼ��������Ϣ��֧�ַ�ҳ��ѯ
        /// <summary>
        /// ��ȡ�ҵ�ԤԼ��������Ϣ��֧�ַ�ҳ��ѯ
        /// </summary>
        /// <param name="loadOptions">��ѯ����</param>
        /// <returns></returns>
        public async Task<object> GetMyListDataAsync(DataSourceLoadOptions loadOptions)
        {
            return base.DataSourceLoadMap(GetDataSource().Where(p => p.UserId == AbpSession.UserId.Value), loadOptions);
        }
        #endregion



        #region ����ID��ȡԤԼ���Ϣ
        /// <summary>
        /// ����ID��ȡԤԼ���Ϣ
        /// </summary>
        /// <param name="id">ԤԼ���Ϣ����</param>
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
                Logger.Error("ԤԼ���ȡ�쳣��", e);
                throw new AbpException("ԤԼ���ȡ�쳣", e);
            }
        }
        #endregion

        #region  �������߸���ԤԼ���Ϣ
        /// <summary>
        /// �������߸���ԤԼ���Ϣ
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
                Logger.Error("ԤԼ������쳣��", e);
                throw new AbpException("ԤԼ������쳣��", e);
            }
        }
        #endregion

        #region  ����ԤԼ���Ϣ
        /// <summary>
        /// ����ԤԼ���Ϣ
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
                Logger.Error("ԤԼ������쳣��", e);
                throw new AbpException("ԤԼ������쳣��", e);
            }
        }
        #endregion

        #region ����ԤԼ���Ĭ�ϲ���
        /// <summary>
        /// ����ԤԼ���Ĭ�ϲ���
        /// </summary>
        /// <returns></returns>
        public async Task<AppointmentActivityEditDto> GetEmptyEntityAsync()
        {
            return await Task.Run(() => new AppointmentActivityEditDto());
        }
        #endregion

        #region ����ɾ��ԤԼ�
        /// <summary>
        /// ����ɾ��ԤԼ�
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
                Logger.Error("ԤԼ�ɾ���쳣��", e);
                throw new AbpException("ԤԼ�ɾ���쳣��", e);
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

            //var list = AppointmentActivityRepository.GetAll().WhereIf(id != 0, p => p.IsDeleted == false && p.StoreId == id).ToList();
            var list = AppointmentActivityRepository.GetAll().AsEnumerable();
            if (id != 0)
                list = list.Where(p => p.IsDeleted == false && p.StoreId == id);
            return new {
                n = "�ԤԼ",
                n1 = "��ЧԤԼ��",
                n2 = "����ϵ",
                n3 = "δ��ϵ",
                t1 = list.Where(p => p.State == 1).Count(), t2 = list.Where(p => p.ContactState == 3).Count(), t3 = list.Where(p => p.ContactState == 0).Count() };
        }
        #endregion
    }

    public class AppointmentActivityEditDto : EntityDto
    {
        /// <summary>
        /// �Id
        /// </summary>
        [Required]
        public int ActivityId { get; set; }

        /// <summary>
        /// �ŵ�Id
        /// </summary>
        [Required]
        public int StoreId { get; set; }

        /// <summary>
        /// �Ա� 0�� 1Ů
        /// </summary>
        public int Sex { get; set; }

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
        /// �û�Id
        /// </summary>
        public long UserId { get; set; }
        /// <summary>
        /// �Id
        /// </summary>
        public int ActivityId { get; set; }

        /// <summary>
        /// �����
        /// </summary>
        public string ActivityTitle { get; set; }

        /// <summary>
        /// �ŵ�Id
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// �ŵ�����
        /// </summary>
        public string StoreName { get; set; }

        /// <summary>
        /// �Ա� 0�� 1Ů
        /// </summary>
        public int Sex { get; set; }

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
        /// �ύʱ��
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
