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
    public class RacingAppService : IFAppServiceBase, IRacingAppService
    {
        IRepository<RacingAggregate> RacingRepository;
        public RacingAppService(
            IRepository<RacingAggregate> RacingRepository)
        {
            this.RacingRepository = RacingRepository;
        }

        #region 获取票务轮播图
        /// <summary>
        /// 获取票务轮播图
        /// </summary>
        public async Task<RacingDto> GetAsync()
        {
            try
            {
                var Racing = await RacingRepository.GetAll().FirstOrDefaultAsync();
                var dto = this.ObjectMapper.Map<RacingDto>(Racing ?? new RacingAggregate());
                return dto;
            }
            catch (Exception e)
            {
                Logger.Error("票务轮播图获取异常！", e);
                throw new AbpException("票务轮播图获取异常", e);
            }
        }
        #endregion

        #region  新增或者更改票务轮播图
        /// <summary>
        /// 新增或者更改票务轮播图
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task SaveAsync(RacingEditDto input)
        {
            try
            {
                RacingAggregate entity = this.ObjectMapper.Map<RacingAggregate>(input);
                RacingAggregate RacingAggregate = RacingRepository.GetAll().FirstOrDefault();
                if (RacingAggregate!=null)
                {
                    RacingAggregate.Imgs = entity.Imgs;
                    RacingAggregate.MobileImgs = entity.MobileImgs;
                    await RacingRepository.UpdateAsync(RacingAggregate);
                }
                else
                {
                    await RacingRepository.InsertAsync(entity);
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            catch (Exception e)
            {
                Logger.Error("票务轮播图保存异常！", e);
                throw new AbpException("票务轮播图保存异常！", e);
            }
        }
        #endregion

    }

    public class RacingEditDto : EntityDto
    {

        /// <summary>
        /// 轮播图
        /// </summary>
        public string Imgs { get; set; }


        /// <summary>
        /// 轮播图移动端
        /// </summary>
        public string MobileImgs { get; set; }
    }

    public class RacingDto : EntityDto
    {
        /// <summary>
        /// 轮播图
        /// </summary>
        public string Imgs { get; set; }

        /// <summary>
        /// 轮播图移动端
        /// </summary>
        public string MobileImgs { get; set; }
    }
}
