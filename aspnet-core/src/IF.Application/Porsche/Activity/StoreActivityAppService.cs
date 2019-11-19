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

namespace IF.Porsche
{
    public class StoreActivityAppService : IFAppServiceBase, IStoreActivityAppService
    {
        IRepository<StoreActivityAggregate> StoreActivityRepository;
        IRepository<StoreAggregate> StoreRepository;
        public StoreActivityAppService(
            IRepository<StoreActivityAggregate> StoreActivityRepository,
            IRepository<StoreAggregate> StoreRepository)
        {
            this.StoreActivityRepository = StoreActivityRepository;
            this.StoreRepository = StoreRepository;
        }
        /// <summary>
        ///  ��ȡ��������Ϣ��֧�ַ�ҳ��ѯ
        /// </summary>
        /// <param name="loadOptions">��ѯ����</param>
        /// <returns></returns>
        public async Task<object> GetListDataAsync(DataSourceLoadOptions loadOptions)
        {
            var result = from StoreActivity in StoreActivityRepository.GetAll().AsEnumerable()
                         join Store in StoreRepository.GetAll().AsEnumerable()
                         on StoreActivity.StoreId equals Store.Id
                         select new StoreActivityDto
                         {
                             Id = StoreActivity.Id,
                             Title = StoreActivity.Title,
                             Desc = StoreActivity.Desc,
                             ImgUrl = StoreActivity.ImgUrl,
                             StoreId = StoreActivity.StoreId,
                             StoreName = Store.Name,
                             CreationTime = StoreActivity.CreationTime
                         };
            return base.DataSourceLoadMap(result, loadOptions);
        }

        #region ����ID��ȡ���Ϣ
        /// <summary>
        /// ����ID��ȡ���Ϣ
        /// </summary>
        /// <param name="id">���Ϣ����</param>
        /// <returns></returns>
        public async Task<StoreActivityDto> GetAsync(int id)
        {
            try
            {
                var StoreActivity = await StoreActivityRepository.GetAll().Where(k => k.Id == id).FirstOrDefaultAsync();

                if (StoreActivity == null)
                {
                    throw new EntityNotFoundException(typeof(StoreActivityAggregate), id);
                }
                var dto = this.ObjectMapper.Map<StoreActivityDto>(StoreActivity);
                return dto;
            }
            catch (Exception e)
            {
                Logger.Error("���ȡ�쳣��", e);
                throw new AbpException("���ȡ�쳣", e);
            }
        }
        #endregion

        #region  �������߸��Ļ��Ϣ
        /// <summary>
        /// �������߸��Ļ��Ϣ
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [AbpAuthorize("Admin.StoreActivity.Edit")]
        public async Task SaveAsync(StoreActivityEditDto input)
        {
            try
            {
                StoreActivityAggregate entity = this.ObjectMapper.Map<StoreActivityAggregate>(input);
                if (input.Id != 0)
                {
                    StoreActivityAggregate StoreActivityAggregate = StoreActivityRepository.GetAll().Where(k => k.Id == input.Id).FirstOrDefault();
                    StoreActivityAggregate.Title = entity.Title;
                    StoreActivityAggregate.Desc = entity.Desc;
                    StoreActivityAggregate.ImgUrl = entity.ImgUrl;
                    StoreActivityAggregate.Detail = entity.Detail;
                    StoreActivityAggregate.StoreId = entity.StoreId;

                    await StoreActivityRepository.UpdateAsync(StoreActivityAggregate);
                }
                else
                {
                    await StoreActivityRepository.InsertAsync(entity);
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            catch (Exception e)
            {
                Logger.Error("������쳣��", e);
                throw new AbpException("������쳣��", e);
            }
        }
        #endregion

        #region �������Ĭ�ϲ���
        /// <summary>
        /// �������Ĭ�ϲ���
        /// </summary>
        /// <returns></returns>
        public async Task<StoreActivityEditDto> GetEmptyEntityAsync()
        {
            return await Task.Run(() => new StoreActivityEditDto());
        }
        #endregion

        #region ����ɾ���
        /// <summary>
        /// ����ɾ���
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete]
        [AbpAuthorize("Admin.StoreActivity.Edit")]
        public async Task BatchDeleteAsync(string ids)
        {
            try
            {
                foreach (var id in ids.Split(','))
                {
                    await StoreActivityRepository.DeleteAsync(kv => kv.Id == int.Parse(id));
                }
                await CurrentUnitOfWork.SaveChangesAsync();

            }
            catch (Exception e)
            {
                Logger.Error("�ɾ���쳣��", e);
                throw new AbpException("�ɾ���쳣��", e);
            }
        }
        #endregion
    }

    public class StoreActivityEditDto : EntityDto
    {

        /// <summary>
        /// ����
        /// </summary>
        [Required]
        public string Title { get; set; }

        /// <summary>
        /// �����ŵ�Id
        /// </summary>
        [Required]
        public int StoreId { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        [StringLength(500)]
        public string Desc { get; set; }

        /// <summary>
        /// ����ͼ
        /// </summary>
        public string ImgUrl { get; set; }


        /// <summary>
        /// ����
        /// </summary>
        public string Detail { get; set; }
    }

    public class StoreActivityDto : EntityDto
    {
        /// <summary>
        /// ����
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// �����ŵ�Id
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// �ŵ�����
        /// </summary>
        public string StoreName { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        /// ����ͼ
        /// </summary>
        public string ImgUrl { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string Detail { get; set; }

        /// <summary>
        /// ����ʱ��
        /// </summary>
        public DateTime CreationTime { get; set; }
    }
}
