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
using DbHelper;
using IF.Configuration;

namespace IF.Porsche
{
    public class StoreWxPayAppService : IFAppServiceBase, IStoreWxPayAppService
    {
        IRepository<StoreWxPayAggregate> StoreWxPayRepository;
        IRepository<DealerAggregate> DealerRepository;
        MSSqlHelper sqlHelper;
        public StoreWxPayAppService(
            IRepository<StoreWxPayAggregate> StoreWxPayRepository,
            IRepository<DealerAggregate> DealerRepository)
        {
            this.StoreWxPayRepository = StoreWxPayRepository;
            this.DealerRepository = DealerRepository;
            sqlHelper = new MSSqlHelper(AppConfigurations.GetAppSettings().GetSection("ConnectionStrings").GetSection("OutSide").Value);
        }
        /// <summary>
        /// ��ȡ������֧���̻��������Ϣ��֧�ַ�ҳ��ѯ
        /// </summary>
        /// <param name="loadOptions">��ѯ����</param>
        /// <returns></returns>
        public async Task<object> GetListDataAsync(DataSourceLoadOptions loadOptions)
        {
            var DealerList = sqlHelper.Query<DealerDto>(
                string.Format(@"select Id,Name,Distance,Address,Phone,Status,Longitude,Latitude,AUserId,DDesc,System_PlaceId from Dealer
                           where Name like '%-PMI'"));
            var result = from StoreWxPay in StoreWxPayRepository.GetAll().AsEnumerable()
                         join Dealer in DealerList
                         on StoreWxPay.StoreId equals Dealer.Id
                         select new StoreWxPayDto
                         {
                             Id = StoreWxPay.Id,
                             StoreId = StoreWxPay.StoreId,
                             StoreName = Dealer.Name.Replace("-PMI", ""),
                             appid = StoreWxPay.appid,
                             secret = StoreWxPay.secret,
                             mch_id = StoreWxPay.mch_id,
                             key = StoreWxPay.key
                         };
            return base.DataSourceLoadMap(result, loadOptions);
        }

        #region ����ID��ȡ������֧���̻���Ϣ
        /// <summary>
        /// ����ID��ȡ������֧���̻���Ϣ
        /// </summary>
        /// <param name="id">������֧���̻���Ϣ����</param>
        /// <returns></returns>
        public async Task<StoreWxPayDto> GetAsync(int id)
        {
            try
            {
                var StoreWxPay = await StoreWxPayRepository.GetAll().Where(k => k.Id == id).FirstOrDefaultAsync();

                if (StoreWxPay == null)
                {
                    throw new EntityNotFoundException(typeof(StoreWxPayAggregate), id);
                }
                var dto = this.ObjectMapper.Map<StoreWxPayDto>(StoreWxPay);
                return dto;
            }
            catch (Exception e)
            {
                Logger.Error("������֧���̻���ȡ�쳣��", e);
                throw new AbpException("������֧���̻���ȡ�쳣", e);
            }
        }
        #endregion

        #region  �������߸��ľ�����֧���̻���Ϣ
        /// <summary>
        /// �������߸��ľ�����֧���̻���Ϣ
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [AbpAuthorize("Admin.Store.Edit")]
        public async Task SaveAsync(StoreWxPayEditDto input)
        {
            try
            {
                StoreWxPayAggregate entity = this.ObjectMapper.Map<StoreWxPayAggregate>(input);
                if (input.Id != 0)
                {
                    StoreWxPayAggregate StoreWxPayAggregate = StoreWxPayRepository.GetAll().Where(k => k.Id == input.Id).FirstOrDefault();
                    StoreWxPayAggregate.appid = entity.appid;
                    StoreWxPayAggregate.secret = entity.secret;
                    StoreWxPayAggregate.mch_id = entity.mch_id;
                    StoreWxPayAggregate.key = entity.key;

                    await StoreWxPayRepository.UpdateAsync(StoreWxPayAggregate);
                }
                else
                {
                    await StoreWxPayRepository.InsertAsync(entity);
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            catch (Exception e)
            {
                Logger.Error("������֧���̻������쳣��", e);
                throw new AbpException("������֧���̻������쳣��", e);
            }
        }
        #endregion

        #region ����������֧���̻���Ĭ�ϲ���
        /// <summary>
        /// ����������֧���̻���Ĭ�ϲ���
        /// </summary>
        /// <returns></returns>
        public async Task<StoreWxPayEditDto> GetEmptyEntityAsync()
        {
            return await Task.Run(() => new StoreWxPayEditDto());
        }
        #endregion

        #region ����ɾ��������֧���̻�
        /// <summary>
        /// ����ɾ��������֧���̻�
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
                    await StoreWxPayRepository.DeleteAsync(kv => kv.Id == int.Parse(id));
                }
                await CurrentUnitOfWork.SaveChangesAsync();

            }
            catch (Exception e)
            {
                Logger.Error("������֧���̻�ɾ���쳣��", e);
                throw new AbpException("������֧���̻�ɾ���쳣��", e);
            }
        }
        #endregion
    }

    public class StoreWxPayEditDto : EntityDto
    {
        /// <summary>
        /// ������Id
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// �����˺�ID
        /// </summary>
        public string appid { get; set; }

        /// <summary>
        /// Ӧ����Կ
        /// </summary>
        public string secret { get; set; }

        /// <summary>
        /// �̻���
        /// </summary>
        public string mch_id { get; set; }

        /// <summary>
        /// ΢��֧��API��Կ
        /// </summary>
        public string key { get; set; }
    }

    public class StoreWxPayDto : EntityDto
    {
        /// <summary>
        /// ������Id
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// ������
        /// </summary>
        public string StoreName { get; set; }

        /// <summary>
        /// �����˺�ID
        /// </summary>
        public string appid { get; set; }

        /// <summary>
        /// Ӧ����Կ
        /// </summary>
        public string secret { get; set; }

        /// <summary>
        /// �̻���
        /// </summary>
        public string mch_id { get; set; }

        /// <summary>
        /// ΢��֧��API��Կ
        /// </summary>
        public string key { get; set; }
    }
}
