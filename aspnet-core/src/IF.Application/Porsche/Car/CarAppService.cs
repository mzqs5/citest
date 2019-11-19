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
        /// 获取车型的相关信息，支持分页查询
        /// </summary>
        /// <param name="loadOptions">查询参数</param>
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

        #region 根据ID获取车型信息
        /// <summary>
        /// 根据ID获取车型信息
        /// </summary>
        /// <param name="id">车型信息主键</param>
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
                Logger.Error("车型获取异常！", e);
                throw new AbpException("车型获取异常", e);
            }
        }
        #endregion

        #region  新增或者更改车型信息
        /// <summary>
        /// 新增或者更改车型信息
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
                Logger.Error("车型保存异常！", e);
                throw new AbpException("车型保存异常！", e);
            }
        }
        #endregion

        #region 新增车型的默认参数
        /// <summary>
        /// 新增车型的默认参数
        /// </summary>
        /// <returns></returns>
        public async Task<CarEditDto> GetEmptyEntityAsync()
        {
            return await Task.Run(() => new CarEditDto());
        }
        #endregion

        #region 批量删除车型
        /// <summary>
        /// 批量删除车型
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
                Logger.Error("车型删除异常！", e);
                throw new AbpException("车型删除异常！", e);
            }
        }
        #endregion
    }

    public class CarEditDto : EntityDto
    {
        /// <summary>
        /// 车型名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 类型 1保时捷，2阿斯顿马丁
        /// </summary>
        public int Type { get; set; }
    }

    public class CarDto : EntityDto
    {
        /// <summary>
        /// 车型名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 类型 1保时捷，2阿斯顿马丁
        /// </summary>
        public int Type { get; set; }
    }
}
