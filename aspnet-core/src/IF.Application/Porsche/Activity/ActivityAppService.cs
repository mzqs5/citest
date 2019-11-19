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
    public class ActivityAppService : IFAppServiceBase, IActivityAppService
    {
        IRepository<ActivityAggregate> ActivityRepository;
        public ActivityAppService(
            IRepository<ActivityAggregate> ActivityRepository)
        {
            this.ActivityRepository = ActivityRepository;
        }
        /// <summary>
        ///  ��ȡ��������Ϣ��֧�ַ�ҳ��ѯ
        /// </summary>
        /// <param name="loadOptions">��ѯ����</param>
        /// <returns></returns>
        public async Task<object> GetListDataAsync(DataSourceLoadOptions loadOptions)
        {
            var result = from Activity in ActivityRepository.GetAll().AsEnumerable()
                         select new ActivityDto
                         {
                             Id = Activity.Id,
                             Title = Activity.Title,
                             Desc = Activity.Desc,
                             Imgs = Activity.Imgs,
                             ImgUrl = Activity.ImgUrl,
                             Link = Activity.Link,
                             Type = Activity.Type,
                             State = Activity.State,
                             Sort = Activity.Sort,
                             Date = Activity.Date,
                             Address = Activity.Address,
                             FromName = Activity.FromName,
                             ETitle = Activity.ETitle,
                             EDesc = Activity.EDesc,
                             EAddress = Activity.EAddress,
                             EFromName = Activity.EFromName,
                             EDate = Activity.EDate,
                             Option = Activity.Option,
                             EOption = Activity.EOption,
                             ThumbnailUrl = Activity.ThumbnailUrl,
                             MobileImgUrl = Activity.MobileImgUrl,
                             MobileThumbnailUrl = Activity.MobileThumbnailUrl,
                             MobileImgs = Activity.MobileImgs
                         };
            return base.DataSourceLoadMap(result, loadOptions);
        }

        #region ����ID��ȡ���Ϣ
        /// <summary>
        /// ����ID��ȡ���Ϣ
        /// </summary>
        /// <param name="id">���Ϣ����</param>
        /// <returns></returns>
        public async Task<ActivityDto> GetAsync(int id)
        {
            try
            {
                var Activity = await ActivityRepository.GetAll().Where(k => k.Id == id).FirstOrDefaultAsync();

                if (Activity == null)
                {
                    throw new EntityNotFoundException(typeof(ActivityAggregate), id);
                }
                var dto = this.ObjectMapper.Map<ActivityDto>(Activity);
                return dto;
            }
            catch (Exception e)
            {
                Logger.Error("���ȡ�쳣��", e);
                throw new AbpException("���ȡ�쳣", e);
            }
        }
        #endregion

        #region ��ȡ������Ϣ
        /// <summary>
        /// ��ȡ������Ϣ
        /// </summary>
        /// <returns></returns>
        public async Task<ActivityDto> GetNewAsync()
        {
            try
            {
                var Activity = await ActivityRepository.GetAll().Where(k => k.Type == 2).OrderByDescending(p => p.CreationTime).FirstOrDefaultAsync();

                if (Activity == null)
                {
                    return null;
                }
                var dto = this.ObjectMapper.Map<ActivityDto>(Activity);
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
        [AbpAuthorize("Admin.Activity.Edit")]
        public async Task SaveAsync(ActivityEditDto input)
        {
            try
            {
                ActivityAggregate entity = this.ObjectMapper.Map<ActivityAggregate>(input);
                if (input.Id != 0)
                {
                    ActivityAggregate ActivityAggregate = ActivityRepository.GetAll().Where(k => k.Id == input.Id).FirstOrDefault();
                    ActivityAggregate.Title = entity.Title;
                    ActivityAggregate.PerDetail = entity.PerDetail;
                    ActivityAggregate.BackDetail = entity.BackDetail;
                    ActivityAggregate.Imgs = entity.Imgs;
                    ActivityAggregate.Desc = entity.Desc;
                    ActivityAggregate.ImgUrl = entity.ImgUrl;
                    ActivityAggregate.Link = entity.Link;
                    ActivityAggregate.Type = entity.Type;
                    ActivityAggregate.State = entity.State;
                    ActivityAggregate.Sort = entity.Sort;
                    ActivityAggregate.Date = entity.Date;
                    ActivityAggregate.Address = entity.Address;
                    ActivityAggregate.FromName = entity.FromName;
                    ActivityAggregate.ETitle = entity.ETitle;
                    ActivityAggregate.EPerDetail = entity.EPerDetail;
                    ActivityAggregate.EBackDetail = entity.EBackDetail;
                    ActivityAggregate.EDesc = entity.EDesc;
                    ActivityAggregate.EDate = entity.EDate;
                    ActivityAggregate.EAddress = entity.EAddress;
                    ActivityAggregate.EFromName = entity.EFromName;
                    ActivityAggregate.IntroDetail = entity.IntroDetail;
                    ActivityAggregate.EIntroDetail = entity.EIntroDetail;
                    ActivityAggregate.Option = entity.Option;
                    ActivityAggregate.EOption = entity.EOption;
                    ActivityAggregate.MobileImgUrl = entity.MobileImgUrl;
                    ActivityAggregate.ThumbnailUrl = entity.ThumbnailUrl;
                    ActivityAggregate.MobileThumbnailUrl = entity.MobileThumbnailUrl;
                    ActivityAggregate.MobileImgs = entity.MobileImgs;

                    await ActivityRepository.UpdateAsync(ActivityAggregate);
                }
                else
                {
                    await ActivityRepository.InsertAsync(entity);
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
        public async Task<ActivityEditDto> GetEmptyEntityAsync()
        {
            return await Task.Run(() => new ActivityEditDto());
        }
        #endregion

        #region ����ɾ���
        /// <summary>
        /// ����ɾ���
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete]
        [AbpAuthorize(PermissionNames.Pages_Users)]
        public async Task BatchDeleteAsync(string ids)
        {
            try
            {
                foreach (var id in ids.Split(','))
                {
                    await ActivityRepository.DeleteAsync(kv => kv.Id == int.Parse(id));
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

    public class ActivityEditDto : EntityDto
    {
        /// <summary>
        /// �����Ӣ��
        /// </summary>
        public string EOption { get; set; }
        /// <summary>
        /// �����
        /// </summary>
        public string Option { get; set; }

        /// <summary>
        /// 0 � 1Ʊ�� 2������Ϣ
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 0 ��Ч 1����
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// �����
        /// </summary>
        [Required]
        public string Title { get; set; }

        /// <summary>
        /// ����ͼ
        /// </summary>
        public string ImgUrl { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        [StringLength(500)]
        public string Desc { get; set; }

        /// <summary>
        /// �ֲ�ͼ
        /// </summary>
        public string Imgs { get; set; }

        /// <summary>
        /// �����2
        /// </summary>
        public string BackDetail { get; set; }

        /// <summary>
        /// �����1
        /// </summary>
        public string PerDetail { get; set; }

        /// <summary>
        /// ���
        /// </summary>
        public string IntroDetail { get; set; }

        /// <summary>
        /// �ƶ��˷���ͼ
        /// </summary>
        public string MobileImgUrl { get; set; }

        /// <summary>
        /// ����ͼ
        /// </summary>
        public string ThumbnailUrl { get; set; }

        /// <summary>
        /// �ƶ�������ͼ
        /// </summary>
        public string MobileThumbnailUrl { get; set; }

        /// <summary>
        /// �ƶ����ֲ�ͼ
        /// </summary>
        public string MobileImgs { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// �ص�
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// ������
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// �����Ӣ��
        /// </summary>
        public string ETitle { get; set; }

        /// <summary>
        /// ����Ӣ��
        /// </summary>
        [StringLength(500)]
        public string EDesc { get; set; }

        /// <summary>
        /// ����Ӣ��
        /// </summary>
        public string EDate { get; set; }

        /// <summary>
        /// �ص�Ӣ��
        /// </summary>
        public string EAddress { get; set; }

        /// <summary>
        /// ������Ӣ��
        /// </summary>
        public string EFromName { get; set; }

        /// <summary>
        /// �����Ӣ��
        /// </summary>
        public string EBackDetail { get; set; }

        /// <summary>
        /// �����Ӣ��
        /// </summary>
        public string EPerDetail { get; set; }

        /// <summary>
        /// ���Ӣ��
        /// </summary>
        public string EIntroDetail { get; set; }
    }

    public class ActivityDto : EntityDto
    {
        /// <summary>
        /// �����Ӣ��
        /// </summary>
        public string EOption { get; set; }
        /// <summary>
        /// �����
        /// </summary>
        public string Option { get; set; }

        /// <summary>
        /// 0 � 1Ʊ�� 2������Ϣ
        /// </summary>
        public int Type { get; set; }

        public string TypeText
        {
            get
            {
                switch (this.Type)
                {
                    case 1:
                        return "Ʊ��";
                    case 2:
                        return "������Ϣ";
                    default:
                        return "�";
                }
            }
        }

        /// <summary>
        /// 0 ��Ч 1����
        /// </summary>
        public int State { get; set; }

        public string StateText
        {
            get
            {
                switch (this.State)
                {
                    case 1:
                        return "����";
                    default:
                        return "��Ч";
                }
            }
        }
        /// <summary>
        /// �����
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// ����ͼ
        /// </summary>
        public string ImgUrl { get; set; }

        /// <summary>
        /// �ƶ��˷���ͼ
        /// </summary>
        public string MobileImgUrl { get; set; }

        /// <summary>
        /// ����ͼ
        /// </summary>
        public string ThumbnailUrl { get; set; }

        /// <summary>
        /// �ƶ�������ͼ
        /// </summary>
        public string MobileThumbnailUrl { get; set; }

        /// <summary>
        /// �ƶ����ֲ�ͼ
        /// </summary>
        [StringLength(4000)]
        public string MobileImgs { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        /// �ֲ�ͼ
        /// </summary>
        public string Imgs { get; set; }

        /// <summary>
        /// �����2
        /// </summary>
        public string BackDetail { get; set; }

        /// <summary>
        /// �����1
        /// </summary>
        public string PerDetail { get; set; }

        /// <summary>
        /// ���
        /// </summary>
        public string IntroDetail { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// �ص�
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// ������
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// �����Ӣ��
        /// </summary>
        public string ETitle { get; set; }

        /// <summary>
        /// ����Ӣ��
        /// </summary>
        public string EDesc { get; set; }

        /// <summary>
        /// ����Ӣ��
        /// </summary>
        public string EDate { get; set; }

        /// <summary>
        /// �ص�Ӣ��
        /// </summary>
        public string EAddress { get; set; }

        /// <summary>
        /// ������Ӣ��
        /// </summary>
        public string EFromName { get; set; }

        /// <summary>
        /// �����Ӣ��
        /// </summary>
        public string EBackDetail { get; set; }

        /// <summary>
        /// �����Ӣ��
        /// </summary>
        public string EPerDetail { get; set; }

        /// <summary>
        /// ���Ӣ��
        /// </summary>
        public string EIntroDetail { get; set; }
    }
}
