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
    public class CarAppService : IFAppServiceBase, ICarAppService
    {
        IRepository<CarAggregate> CarRepository;
        public CarAppService(
            IRepository<CarAggregate> CarRepository)
        {
            this.CarRepository = CarRepository;
        }
        /// <summary>
        /// ��ȡ���͵������Ϣ��֧�ַ�ҳ��ѯ
        /// </summary>
        /// <param name="loadOptions">��ѯ����</param>
        /// <returns></returns>
        public async Task<object> GetListDataAsync(DataSourceLoadOptions loadOptions)
        {
            var result = from Car in CarRepository.GetAll().AsEnumerable()
                         select new CarDto
                         {
                             Id = Car.Id,
                             Name = Car.Name,
                             Type = Car.Type
                         };
            return base.DataSourceLoadMap(result, loadOptions);
        }

        #region ����ID��ȡ������Ϣ
        /// <summary>
        /// ����ID��ȡ������Ϣ
        /// </summary>
        /// <param name="id">������Ϣ����</param>
        /// <returns></returns>
        public async Task<CarDto> GetAsync(int id)
        {
            try
            {
                var Car = await CarRepository.GetAll().Where(k => k.Id == id).FirstOrDefaultAsync();

                if (Car == null)
                {
                    throw new EntityNotFoundException(typeof(CarAggregate), id);
                }
                var dto = this.ObjectMapper.Map<CarDto>(Car);
                return dto;
            }
            catch (Exception e)
            {
                Logger.Error("���ͻ�ȡ�쳣��", e);
                throw new AbpException("���ͻ�ȡ�쳣", e);
            }
        }
        #endregion

        #region  �������߸��ĳ�����Ϣ
        /// <summary>
        /// �������߸��ĳ�����Ϣ
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [AbpAuthorize(PermissionNames.Pages_Users)]
        public async Task SaveAsync(CarEditDto input)
        {
            try
            {
                CarAggregate entity = this.ObjectMapper.Map<CarAggregate>(input);
                if (input.Id != 0)
                {
                    CarAggregate CarAggregate = CarRepository.GetAll().Where(k => k.Id == input.Id).FirstOrDefault();
                    CarAggregate.Name = entity.Name;
                    CarAggregate.Type = entity.Type;

                    await CarRepository.UpdateAsync(CarAggregate);
                }
                else
                {
                    await CarRepository.InsertAsync(entity);
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            catch (Exception e)
            {
                Logger.Error("���ͱ����쳣��", e);
                throw new AbpException("���ͱ����쳣��", e);
            }
        }
        #endregion

        #region �������͵�Ĭ�ϲ���
        /// <summary>
        /// �������͵�Ĭ�ϲ���
        /// </summary>
        /// <returns></returns>
        public async Task<CarEditDto> GetEmptyEntityAsync()
        {
            return await Task.Run(() => new CarEditDto());
        }
        #endregion

        #region ����ɾ������
        /// <summary>
        /// ����ɾ������
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
                    await CarRepository.DeleteAsync(kv => kv.Id == int.Parse(id));
                }
                await CurrentUnitOfWork.SaveChangesAsync();

            }
            catch (Exception e)
            {
                Logger.Error("����ɾ���쳣��", e);
                throw new AbpException("����ɾ���쳣��", e);
            }
        }
        #endregion
    }

    public class CarEditDto : EntityDto
    {
        /// <summary>
        /// ��������
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ���� 1��ʱ�ݣ�2��˹����
        /// </summary>
        public int Type { get; set; }
    }

    public class CarDto : EntityDto
    {
        /// <summary>
        /// ��������
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ���� 1��ʱ�ݣ�2��˹����
        /// </summary>
        public int Type { get; set; }
    }
}
