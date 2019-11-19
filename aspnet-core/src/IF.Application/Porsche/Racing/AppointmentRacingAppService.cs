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
    public class AppointmentRacingAppService : IFAppServiceBase, IAppointmentRacingAppService
    {
        IRepository<AppointmentRacingAggregate> AppointmentRacingRepository;
        IRepository<User, long> UserRepository;
        public AppointmentRacingAppService(
            IRepository<AppointmentRacingAggregate> AppointmentRacingRepository,
            IRepository<User, long> UserRepository)
        {
            this.AppointmentRacingRepository = AppointmentRacingRepository;
            this.UserRepository = UserRepository;
        }

        #region ��ȡԤԼ���µ������Ϣ��֧�ַ�ҳ��ѯ
        /// <summary>
        /// ��ȡԤԼ���µ������Ϣ��֧�ַ�ҳ��ѯ
        /// </summary>
        /// <param name="loadOptions">��ѯ����</param>
        /// <returns></returns>
        public async Task<object> GetListDataAsync(DataSourceLoadOptions loadOptions)
        {
            var result = from AppointmentRacing in AppointmentRacingRepository.GetAll().AsEnumerable()
                         select new AppointmentRacingDto
                         {
                             Id = AppointmentRacing.Id,
                             Name = AppointmentRacing.Name,
                             Option = AppointmentRacing.Option,
                             Mobile = AppointmentRacing.Mobile,
                             SurName = AppointmentRacing.SurName,
                             RacingId = AppointmentRacing.RacingId,
                             UserId = AppointmentRacing.UserId,
                             CreationTime = AppointmentRacing.CreationTime
                         };
            loadOptions.Sort = new SortingInfo[] { new SortingInfo() { Selector = "CreationTime", Desc = true } };
            return base.DataSourceLoadMap(result, loadOptions);
        }
        #endregion

        #region ��ȡ�ҵ�ԤԼ���µ������Ϣ��֧�ַ�ҳ��ѯ
        /// <summary>
        /// ��ȡ�ҵ�ԤԼ���µ������Ϣ��֧�ַ�ҳ��ѯ
        /// </summary>
        /// <param name="loadOptions">��ѯ����</param>
        /// <returns></returns>
        public async Task<object> GetMyListDataAsync(DataSourceLoadOptions loadOptions)
        {
            var result = from AppointmentRacing in AppointmentRacingRepository.GetAll()
                         .Where(p => p.UserId == AbpSession.UserId.Value).AsEnumerable()
                         select new AppointmentRacingDto
                         {
                             Id = AppointmentRacing.Id,
                             Name = AppointmentRacing.Name,
                             Option = AppointmentRacing.Option,
                             Mobile = AppointmentRacing.Mobile,
                             SurName = AppointmentRacing.SurName,
                             RacingId = AppointmentRacing.RacingId,
                             UserId = AppointmentRacing.UserId,
                             CreationTime = AppointmentRacing.CreationTime
                         };
            loadOptions.Sort = new SortingInfo[] { new SortingInfo() { Selector = "CreationTime", Desc = true } };
            return base.DataSourceLoadMap(result, loadOptions);
        }
        #endregion

        #region ����ID��ȡԤԼ������Ϣ
        /// <summary>
        /// ����ID��ȡԤԼ������Ϣ
        /// </summary>
        /// <param name="id">ԤԼ������Ϣ����</param>
        /// <returns></returns>
        public async Task<AppointmentRacingDto> GetAsync(int id)
        {
            try
            {
                var AppointmentRacing = await AppointmentRacingRepository.GetAll().Where(k => k.Id == id).FirstOrDefaultAsync();

                if (AppointmentRacing == null)
                {
                    throw new EntityNotFoundException(typeof(AppointmentRacingAggregate), id);
                }
                var dto = this.ObjectMapper.Map<AppointmentRacingDto>(AppointmentRacing);
                return dto;
            }
            catch (Exception e)
            {
                Logger.Error("ԤԼ���»�ȡ�쳣��", e);
                throw new AbpException("ԤԼ���»�ȡ�쳣", e);
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
        public async Task SaveAsync(AppointmentRacingEditDto input)
        {
            try
            {
                AppointmentRacingAggregate entity = this.ObjectMapper.Map<AppointmentRacingAggregate>(input);
                entity.UserId = AbpSession.UserId.Value;
                var user = await UserRepository.GetAll().Include(p => p.UserCars).FirstOrDefaultAsync(p => p.Id == AbpSession.UserId.Value);
                user.Name = input.Name;
                user.Surname = input.SurName;
                await UserRepository.UpdateAsync(user);
                if (input.Id != 0)
                {
                    AppointmentRacingAggregate AppointmentRacingAggregate = AppointmentRacingRepository.GetAll().Where(k => k.Id == input.Id).FirstOrDefault();
                    AppointmentRacingAggregate.SurName = entity.SurName;
                    AppointmentRacingAggregate.Name = entity.Name;
                    AppointmentRacingAggregate.Mobile = entity.Mobile;
                    AppointmentRacingAggregate.Option = entity.Option;
                    AppointmentRacingAggregate.RacingId = entity.RacingId;

                    await AppointmentRacingRepository.UpdateAsync(AppointmentRacingAggregate);
                }
                else
                {
                    await AppointmentRacingRepository.InsertAsync(entity);
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            catch (Exception e)
            {
                Logger.Error("ԤԼ���±����쳣��", e);
                throw new AbpException("ԤԼ���±����쳣��", e);
            }
        }
        #endregion

        #region ����ԤԼ���µ�Ĭ�ϲ���
        /// <summary>
        /// ����ԤԼ���µ�Ĭ�ϲ���
        /// </summary>
        /// <returns></returns>
        public async Task<AppointmentRacingEditDto> GetEmptyEntityAsync()
        {
            return await Task.Run(() => new AppointmentRacingEditDto());
        }
        #endregion

        #region ����ɾ��ԤԼ����
        /// <summary>
        /// ����ɾ��ԤԼ����
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
                    await AppointmentRacingRepository.DeleteAsync(kv => kv.Id == int.Parse(id));
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
    }

    public class AppointmentRacingEditDto : EntityDto
    {

        /// <summary>
        /// ����Id
        /// </summary>
        [Required]
        public int RacingId { get; set; }
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
        /// ��Ʊ����
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Option { get; set; }
    }

    public class AppointmentRacingDto : EntityDto
    {
        /// <summary>
        /// �û�Id
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// ����Id
        /// </summary>
        public int RacingId { get; set; }

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
        /// ��Ʊ����
        /// </summary>
        public string Option { get; set; }

        /// <summary>
        /// ԤԼ����
        /// </summary>
        public string AppointmentType { get { return "Racing"; } }

        /// <summary>
        /// ����ʱ��
        /// </summary>
        public DateTime CreationTime { get; set; }
    }
}
