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

        #region ��ȡԤԼ׷��ŷ�޵������Ϣ��֧�ַ�ҳ��ѯ
        /// <summary>
        /// ��ȡԤԼ׷��ŷ�޵������Ϣ��֧�ַ�ҳ��ѯ
        /// </summary>
        /// <param name="loadOptions">��ѯ����</param>
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

        #region ��ȡ�ҵ�ԤԼ׷��ŷ�޵������Ϣ��֧�ַ�ҳ��ѯ
        /// <summary>
        /// ��ȡ�ҵ�ԤԼ׷��ŷ�޵������Ϣ��֧�ַ�ҳ��ѯ
        /// </summary>
        /// <param name="loadOptions">��ѯ����</param>
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

        #region ����ID��ȡԤԼ׷��ŷ����Ϣ
        /// <summary>
        /// ����ID��ȡԤԼ׷��ŷ����Ϣ
        /// </summary>
        /// <param name="id">ԤԼ׷��ŷ����Ϣ����</param>
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
                Logger.Error("ԤԼ׷��ŷ�޻�ȡ�쳣��", e);
                throw new AbpException("ԤԼ׷��ŷ�޻�ȡ�쳣", e);
            }
        }
        #endregion

        #region  �������߸���ԤԼ׷��ŷ����Ϣ
        /// <summary>
        /// �������߸���ԤԼ׷��ŷ����Ϣ
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
                Logger.Error("ԤԼ׷��ŷ�ޱ����쳣��", e);
                throw new AbpException("ԤԼ׷��ŷ�ޱ����쳣��", e);
            }
        }
        #endregion

        #region ����ԤԼ׷��ŷ�޵�Ĭ�ϲ���
        /// <summary>
        /// ����ԤԼ׷��ŷ�޵�Ĭ�ϲ���
        /// </summary>
        /// <returns></returns>
        public async Task<AppointmentEuroDriveEditDto> GetEmptyEntityAsync()
        {
            return await Task.Run(() => new AppointmentEuroDriveEditDto());
        }
        #endregion

        #region ����ɾ��ԤԼ׷��ŷ��
        /// <summary>
        /// ����ɾ��ԤԼ׷��ŷ��
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
                Logger.Error("ԤԼ׷��ŷ��ɾ���쳣��", e);
                throw new AbpException("ԤԼ׷��ŷ��ɾ���쳣��", e);
            }
        }
        #endregion
    }

    public class AppointmentEuroDriveEditDto : EntityDto
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

    public class AppointmentEuroDriveDto : EntityDto
    {
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
        /// ԤԼ����
        /// </summary>
        public string AppointmentType { get { return "EuroDrive"; } }

        /// <summary>
        /// ����ʱ��
        /// </summary>
        public DateTime CreationTime { get; set; }
    }
}
