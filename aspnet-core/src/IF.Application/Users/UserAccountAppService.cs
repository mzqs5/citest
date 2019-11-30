using Abp;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using DevExtreme.AspNet.Mvc;
using IF.Authorization.Accounts;
using IF.Authorization.Users;
using IF.Porsche;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IF.Authorization
{
    public class UserAccountAppService : IFAppServiceBase, IUserAccountAppService
    {
        IRepository<User, long> UserRepository;
        IRepository<OrderAggregate> OrderRepository;
        IRepository<StoreAggregate> StoreRepository;
        UserManager userManager;
        public UserAccountAppService(
            IRepository<User, long> UserRepository,
            IRepository<OrderAggregate> OrderRepository,
            IRepository<StoreAggregate> StoreRepository,
            UserManager userManager)
        {
            this.userManager = userManager;
            this.StoreRepository = StoreRepository;
            this.UserRepository = UserRepository;
            this.OrderRepository = OrderRepository;
        }

        /// <summary>
        /// 获取用户信息的相关信息，支持分页查询
        /// </summary>
        /// <param name="loadOptions">查询参数</param>
        /// <returns></returns>
        public async Task<object> GetListDataAsync(DataSourceLoadOptions loadOptions)
        {
            var result = from user in UserRepository.GetAll().Include(p => p.Roles).Where(p => p.Roles.Select(r => r.RoleId).Contains(3)).Include(p => p.Contacts).Include(p => p.UserCars).Include(p => p.Wechats).Include(p => p.Address).AsEnumerable()
                         join Store in StoreRepository.GetAll().AsEnumerable()
                         on user.LastStoreId equals Store.Id
                         into Stores
                         from Store in Stores.DefaultIfEmpty()
                         select new UserAccountDto
                         {
                             Id = user.Id,
                             Name = user.Name,
                             UserName = user.UserName,
                             Surname = user.Surname,
                             Mobile = user.Mobile,
                             Sex = user.Sex,
                             IsCarOwner = user.IsCarOwner,
                             LastStoreId = user.LastStoreId,
                             StoreName = Store == null ? "" : Store.Name,
                             StoreId = user.StoreId,
                             CreationTime = user.CreationTime,
                             Contacts = this.ObjectMapper.Map<List<ContactsDto>>(user.Contacts),
                             UserCars = this.ObjectMapper.Map<List<UserCarDto>>(user.UserCars),
                             Wechats = this.ObjectMapper.Map<List<WechatDto>>(user.Wechats),
                             Address = this.ObjectMapper.Map<List<AddressDto>>(user.Address)
                         };
            return base.DataSourceLoadMap(result, loadOptions);
        }

        #region 导出
        [HttpGet]
        [AbpAuthorize("Admin.Members.Export")]
        public async Task<IActionResult> Export(DataSourceLoadOptions loadOptions)
        {
            var result = from user in UserRepository.GetAll().Include(p => p.Roles).Where(p => p.Roles.Select(r => r.RoleId).Contains(3)).Include(p => p.Contacts).Include(p => p.UserCars).Include(p => p.Wechats).Include(p => p.Address).AsEnumerable()
                         select new UserAccountDto
                         {
                             Id = user.Id,
                             Name = user.Name,
                             UserName = user.UserName,
                             Surname = user.Surname,
                             Mobile = user.Mobile,
                             Contacts = this.ObjectMapper.Map<List<ContactsDto>>(user.Contacts),
                             UserCars = this.ObjectMapper.Map<List<UserCarDto>>(user.UserCars),
                             Wechats = this.ObjectMapper.Map<List<WechatDto>>(user.Wechats),
                             Address = this.ObjectMapper.Map<List<AddressDto>>(user.Address)
                         };
            var Export = new ExportHelper();
            Export.AddColumn("UserName", "用户名");
            Export.AddColumn("Surname", "姓");
            Export.AddColumn("Name", "名");
            Export.AddColumn("Mobile", "手机号码");
            Export.AddColumn("Contacts", "联系人");
            Export.AddColumn("UserCars", "用户车辆");
            Export.AddColumn("Wechats", "微信信息");
            Export.AddColumn("Address", "用户地址");
            var result1 = Export.ExportExcel<UserAccountDto>(base.DataSourceLoadMap(result, loadOptions).data);
            result1.FileDownloadName = "会员记录";
            return result1;
        }

        #endregion

        #region 根据ID获取用户信息信息
        /// <summary>
        /// 根据ID获取用户信息信息
        /// </summary>
        /// <param name="id">用户信息信息主键</param>
        /// <returns></returns>
        public async Task<UserAccountDto> GetAsync(long id)
        {
            try
            {
                var user = await UserRepository.GetAll().Include(p=>p.Wechats).Include(p=>p.Contacts).Include(p=>p.Address).Include(p=>p.UserCars).Include(p=>p.UserCModels).Where(k => k.Id == id).FirstOrDefaultAsync();

                if (user == null)
                {
                    throw new EntityNotFoundException(typeof(User), id);
                }
                var dto = this.ObjectMapper.Map<UserAccountDto>(user);
                return dto;
            }
            catch (Exception e)
            {
                Logger.Error("用户信息获取异常！", e);
                throw new AbpException("用户信息获取异常", e);
            }
        }
        #endregion

        #region  新增或者更改用户信息信息
        /// <summary>
        /// 新增或者更改用户信息信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [AbpAuthorize("Admin.Members.Edit")]
        public async Task SaveAsync(UserAccountEditDto input)
        {
            try
            {
                User entity = this.ObjectMapper.Map<User>(input);
                if (input.Id != 0)
                {
                    User user = UserRepository.GetAll().Where(k => k.Id == input.Id).FirstOrDefault();
                    user.Name = entity.Name;
                    user.Surname = entity.Surname;
                    user.Mobile = entity.Mobile;
                    await UserRepository.UpdateAsync(user);
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                Logger.Error("用户信息保存异常！", e);
                throw new AbpException("用户信息保存异常！", e);
            }
        }
        #endregion

        #region  更新用户车辆车架号
        /// <summary>
        /// 更新用户车辆车架号
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [AbpAuthorize("Admin.Members.Edit")]
        public async Task SaveUserCarAsync(UpdateUserCarEditDto input)
        {
            try
            {
                User user = UserRepository.GetAll().Include(p => p.UserCars).Where(k => k.Id == input.Id).FirstOrDefault();
                var userCar = user.UserCars.FirstOrDefault(p => p.Id == input.UserCarId);
                if (userCar == null)
                    throw new AbpException("未找到该用户的车辆,可能数据已发生变化！");
                userCar.FrameNo = input.FrameNo;
                await UserRepository.UpdateAsync(user);
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Logger.Error("车辆车架号保存异常！", e);
                throw new AbpException("车辆车架号保存异常！", e);
            }
        }
        #endregion

        #region 批量删除用户信息
        /// <summary>
        /// 批量删除用户信息
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete]
        [AbpAuthorize("Admin.Members.Delete")]
        public async Task BatchDeleteAsync(string ids)
        {
            try
            {
                foreach (var id in ids.Split(','))
                {
                    if (OrderRepository.GetAll().Any(p => p.UserId == int.Parse(id)))
                        throw new AbpException("该用户存在订单，不能删除！");

                    await UserRepository.DeleteAsync(kv => kv.Id == int.Parse(id));
                }
                await CurrentUnitOfWork.SaveChangesAsync();

            }
            catch (AbpException e)
            {
                Logger.Error("用户信息删除异常！", e);
                throw e;
            }
        }
        #endregion
        
    }

    public class UpdateUserCarEditDto : EntityDto<long>
    {
        /// <summary>
        /// 车架号
        /// </summary>
        public string FrameNo { get; set; }

        public int UserCarId { get; set; }
    }

    public class UserAccountEditDto : EntityDto<long>
    {
        /// <summary>
        /// 名
        /// </summary>
        public new string Name { get; set; }
        /// <summary>
        /// 姓
        /// </summary>
        public new string Surname { get; set; }

        /// <summary>
        /// 性别 0男 1女
        /// </summary>
        public int Sex { get; set; }
        /// <summary>
        /// 是否车主 0否 1是
        /// </summary>
        public int IsCarOwner { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 所属门店
        /// </summary>
        public int LastStoreId { get; set; }
    }

    public class UserAccountDto : EntityDto<long>
    {
        public string UserName { get; set; }
        public string Name { get; set; }

        public string Surname { get; set; }
        public string FullName { get { return (this.Surname + " " + this.Name); } }

        public int Sex { get; set; }

        public int IsCarOwner { get; set; }
        public string Mobile { get; set; }

        public string UnionId { get; set; }

        public int LastStoreId { get; set; }

        public string StoreName { get; set; }
        public int StoreId { get; set; }

        public List<WechatDto> Wechats { get; set; }

        public List<UserCarDto> UserCars { get; set; }

        public List<UserCModelDto> UserCModels { get; set; }

        public List<ContactsDto> Contacts { get; set; }

        public List<AddressDto> Address { get; set; }
        public DateTime CreationTime { get; set; }
    }
}

