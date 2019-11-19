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
    public class CModelAppService : IFAppServiceBase, ICModelAppService
    {
        IRepository<CModelAggregate> CModelRepository;
        public CModelAppService(
            IRepository<CModelAggregate> CModelRepository)
        {
            this.CModelRepository = CModelRepository;
        }
        /// <summary>
        /// 获取车型的相关信息，支持分页查询
        /// </summary>
        /// <param name="loadOptions">查询参数</param>
        /// <returns></returns>
        public async Task<object> GetListDataAsync(DataSourceLoadOptions loadOptions)
        {
            var result = from CModel in CModelRepository.GetAll().AsEnumerable()
                         select new CModelDto
                         {
                             Id = CModel.Id,
                             Name = CModel.Name,
                             BgPic = CModel.BgPic,
                             Pic = CModel.Pic
                         };
            return base.DataSourceLoadMap(result, loadOptions);
        }

        #region 根据ID获取车型信息
        /// <summary>
        /// 根据ID获取车型信息
        /// </summary>
        /// <param name="id">车型信息主键</param>
        /// <returns></returns>
        public async Task<CModelDto> GetAsync(int id)
        {
            try
            {
                var CModel = await CModelRepository.GetAll().Where(k => k.Id == id).FirstOrDefaultAsync();

                if (CModel == null)
                {
                    throw new EntityNotFoundException(typeof(CModelAggregate), id);
                }
                var dto = this.ObjectMapper.Map<CModelDto>(CModel);
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
        public async Task SaveAsync(CModelEditDto input)
        {
            try
            {
                CModelAggregate entity = this.ObjectMapper.Map<CModelAggregate>(input);
                if (input.Id != 0)
                {
                    CModelAggregate CModelAggregate = CModelRepository.GetAll().Where(k => k.Id == input.Id).FirstOrDefault();
                    CModelAggregate.Name = entity.Name;
                    CModelAggregate.Pic = entity.Pic;
                    CModelAggregate.BgPic = entity.BgPic;

                    await CModelRepository.UpdateAsync(CModelAggregate);
                }
                else
                {
                    await CModelRepository.InsertAsync(entity);
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
        public async Task<CModelEditDto> GetEmptyEntityAsync()
        {
            return await Task.Run(() => new CModelEditDto());
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
                    await CModelRepository.DeleteAsync(kv => kv.Id == int.Parse(id));
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

    public class CModelEditDto : EntityDto
    {
        /// <summary>
        /// 车型名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 大图
        /// </summary>
        public string BgPic { get; set; }

        /// <summary>
        /// 小图
        /// </summary>
        public string Pic { get; set; }
    }

    public class CModelDto : EntityDto
    {
        /// <summary>
        /// 车型名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 大图
        /// </summary>
        public string BgPic { get; set; }

        /// <summary>
        /// 小图
        /// </summary>
        public string Pic { get; set; }
    }
}
