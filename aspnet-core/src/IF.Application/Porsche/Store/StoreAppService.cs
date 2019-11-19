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

namespace IF.Porsche
{
    public class StoreAppService : IFAppServiceBase, IStoreAppService
    {
        IRepository<StoreAggregate> StoreRepository;
        public StoreAppService(
            IRepository<StoreAggregate> StoreRepository)
        {
            this.StoreRepository = StoreRepository;
        }
        /// <summary>
        /// ��ȡ�����̵������Ϣ��֧�ַ�ҳ��ѯ
        /// </summary>
        /// <param name="loadOptions">��ѯ����</param>
        /// <returns></returns>
        public async Task<object> GetListDataAsync(DataSourceLoadOptions loadOptions)
        {
            var result = from Store in StoreRepository.GetAll().AsEnumerable()
                         select new StoreDto
                         {
                             Id = Store.Id,
                             Name = Store.Name,
                             Date = Store.Date,
                             Address = Store.Address,
                             Phone = Store.Phone,
                             More = Store.More,
                             EName = Store.EName,
                             EDate = Store.EDate,
                             EAddress = Store.EAddress,
                             EPhone = Store.EPhone,
                             EMore = Store.EMore,
                             Type = Store.Type
                         };
            return base.DataSourceLoadMap(result, loadOptions);
        }

        #region ����ID��ȡ��������Ϣ
        /// <summary>
        /// ����ID��ȡ��������Ϣ
        /// </summary>
        /// <param name="id">��������Ϣ����</param>
        /// <returns></returns>
        public async Task<StoreDto> GetAsync(int id)
        {
            try
            {
                var Store = await StoreRepository.GetAll().Where(k => k.Id == id).FirstOrDefaultAsync();

                if (Store == null)
                {
                    throw new EntityNotFoundException(typeof(StoreAggregate), id);
                }
                var dto = this.ObjectMapper.Map<StoreDto>(Store);
                return dto;
            }
            catch (Exception e)
            {
                Logger.Error("�����̻�ȡ�쳣��", e);
                throw new AbpException("�����̻�ȡ�쳣", e);
            }
        }
        #endregion

        #region  �������߸��ľ�������Ϣ
        /// <summary>
        /// �������߸��ľ�������Ϣ
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [AbpAuthorize("Admin.Store.Edit")]
        public async Task SaveAsync(StoreEditDto input)
        {
            try
            {
                StoreAggregate entity = this.ObjectMapper.Map<StoreAggregate>(input);
                if (input.Id != 0)
                {
                    StoreAggregate StoreAggregate = StoreRepository.GetAll().Where(k => k.Id == input.Id).FirstOrDefault();
                    StoreAggregate.Name = entity.Name;
                    StoreAggregate.Phone = entity.Phone;
                    StoreAggregate.Address = entity.Address;
                    StoreAggregate.Date = entity.Date;
                    StoreAggregate.More = entity.More;
                    StoreAggregate.EName = entity.EName;
                    StoreAggregate.EPhone = entity.EPhone;
                    StoreAggregate.EAddress = entity.EAddress;
                    StoreAggregate.EDate = entity.EDate;
                    StoreAggregate.EMore = entity.EMore;
                    StoreAggregate.Type = entity.Type;
                    await StoreRepository.UpdateAsync(StoreAggregate);
                }
                else
                {
                    await StoreRepository.InsertAsync(entity);
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            catch (Exception e)
            {
                Logger.Error("�����̱����쳣��", e);
                throw new AbpException("�����̱����쳣��", e);
            }
        }
        #endregion

        #region ���������̵�Ĭ�ϲ���
        /// <summary>
        /// ���������̵�Ĭ�ϲ���
        /// </summary>
        /// <returns></returns>
        public async Task<StoreEditDto> GetEmptyEntityAsync()
        {
            return await Task.Run(() => new StoreEditDto());
        }
        #endregion

        #region ����ɾ��������
        /// <summary>
        /// ����ɾ��������
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete]
        [AbpAuthorize("Admin.Store.Edit")]
        public async Task BatchDeleteAsync(string ids)
        {
            try
            {
                foreach (var id in ids.Split(','))
                {
                    await StoreRepository.DeleteAsync(kv => kv.Id == int.Parse(id));
                }
                await CurrentUnitOfWork.SaveChangesAsync();

            }
            catch (Exception e)
            {
                Logger.Error("������ɾ���쳣��", e);
                throw new AbpException("������ɾ���쳣��", e);
            }
        }
        #endregion
    }

    public class StoreEditDto : EntityDto
    {
        /// <summary>
        /// ����������
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ���������� Ӣ��
        /// </summary>
        public string EName { get; set; }

        /// <summary>
        /// �����̵绰
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// �����̵绰Ӣ��
        /// </summary>
        public string EPhone { get; set; }

        /// <summary>
        /// ��ַ
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// ��ַӢ��
        /// </summary>
        public string EAddress { get; set; }

        /// <summary>
        /// Ӫҵʱ��
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// Ӫҵʱ��Ӣ��
        /// </summary>
        public string EDate { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string More { get; set; }


        /// <summary>
        /// ����Ӣ��
        /// </summary>
        public string EMore { get; set; }

        /// <summary>
        /// ���� 1��ʱ�ݣ�2��˹����
        /// </summary>
        public int Type { get; set; }
    }

    public class StoreDto : EntityDto
    {
        /// <summary>
        /// ����������
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ���������� Ӣ��
        /// </summary>
        public string EName { get; set; }

        /// <summary>
        /// �����̵绰
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// �����̵绰Ӣ��
        /// </summary>
        public string EPhone { get; set; }

        /// <summary>
        /// ��ַ
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// ��ַӢ��
        /// </summary>
        public string EAddress { get; set; }

        /// <summary>
        /// Ӫҵʱ��
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// Ӫҵʱ��Ӣ��
        /// </summary>
        public string EDate { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string More { get; set; }


        /// <summary>
        /// ����Ӣ��
        /// </summary>
        public string EMore { get; set; }

        /// <summary>
        /// ���� 1��ʱ�ݣ�2��˹����
        /// </summary>
        public int Type { get; set; }
    }
}
