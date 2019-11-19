using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Zero.Configuration;
using IF.Authorization.Accounts.Dto;
using IF.Authorization.Users;
using IF.Configuration;
using IF.Porsche;
using IF.Users.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IF.Authorization.Accounts
{
    /// <summary>
    /// 无需鉴权
    /// </summary>
    public class AccountAppService : IFAppServiceBase, IAccountAppService
    {
        private readonly UserRegistrationManager _userRegistrationManager;
        IRepository<SmsAggregate> SmsRepository;
        IRepository<AppointmentAggregate> AppointmentRepository;
        IRepository<AppointmentEuroDriveAggregate> AppointmentEuroDriveRepository;
        IRepository<AppointmentRacingAggregate> AppointmentRacingRepository;
        IRepository<AppointmentTestDriveAggregate> AppointmentTestDrivRepository;
        IRepository<OrderAggregate> OrderRepository;
        IRepository<ShoppingCartAggregate> ShoppingCartRepository;
        IRepository<User, long> UserRepository;
        IRepository<CarAggregate, int> CarRepository;
        IRepository<CModelAggregate, int> CModelRepository;
        UserManager userManager;
        IRepository<AppointmentWebActivityAggregate> AppointmentWebActivityRepository;
        public AccountAppService(
            UserRegistrationManager userRegistrationManager,
            IRepository<SmsAggregate> SmsRepository,
            IRepository<AppointmentAggregate> AppointmentRepository,
            IRepository<AppointmentWebActivityAggregate> AppointmentWebActivityRepository,
            IRepository<AppointmentEuroDriveAggregate> AppointmentEuroDriveRepository,
            IRepository<AppointmentRacingAggregate> AppointmentRacingRepository,
            IRepository<AppointmentTestDriveAggregate> AppointmentTestDrivRepository,
            IRepository<OrderAggregate> OrderRepository,
            IRepository<User, long> UserRepository,
            IRepository<ShoppingCartAggregate> ShoppingCartRepository,
            IRepository<CarAggregate, int> CarRepository,
            IRepository<CModelAggregate, int> CModelRepository,
            UserManager userManager)
        {
            _userRegistrationManager = userRegistrationManager;
            this.SmsRepository = SmsRepository;
            this.AppointmentRepository = AppointmentRepository;
            this.AppointmentEuroDriveRepository = AppointmentEuroDriveRepository;
            this.AppointmentRacingRepository = AppointmentRacingRepository;
            this.AppointmentTestDrivRepository = AppointmentTestDrivRepository;
            this.OrderRepository = OrderRepository;
            this.ShoppingCartRepository = ShoppingCartRepository;
            this.userManager = userManager;
            this.UserRepository = UserRepository;
            this.CarRepository = CarRepository;
            this.CModelRepository = CModelRepository;
            this.AppointmentWebActivityRepository = AppointmentWebActivityRepository;
        }

        /// <summary>
        /// 用户注册（无需鉴权）
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<RegisterOutput> Register(RegisterInput input)
        {
            var Sms = await SmsRepository.GetAll().Where(k => k.Mobile == input.Mobile
                    && k.Code == input.Code
                    && k.CreationTime.AddMinutes((Double)k.EffectiveTime) >= DateTime.Now)
                    .FirstOrDefaultAsync();

            if (Sms == null)
            {
                Logger.Error("短信验证失败！");
                throw new AbpException("短信验证失败!");
            }
            var user = await _userRegistrationManager.RegisterAsync(
                input.Mobile,
                input.Password);

            var isEmailConfirmationRequiredForLogin = await SettingManager.GetSettingValueAsync<bool>(AbpZeroSettingNames.UserManagement.IsEmailConfirmationRequiredForLogin);

            return new RegisterOutput
            {
                CanLogin = user.IsActive && (user.IsEmailConfirmed || !isEmailConfirmationRequiredForLogin)
            };
        }

        #region 联系人管理
        /// <summary>
        /// 添加联系人(需鉴权【普通用户】)
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task AddContacts(AddContactsDto dto)
        {
            if (!AbpSession.UserId.HasValue)
                throw new AbpException("错误的请求");
            var Sms = await SmsRepository.GetAll().Where(k => k.Mobile == dto.Mobile
                    && k.Code == dto.Code
                    && k.CreationTime.AddMinutes((Double)k.EffectiveTime) >= DateTime.Now)
                    .FirstOrDefaultAsync();
            if (Sms == null)
            {
                Logger.Error("短信验证失败！");
                throw new AbpException("短信验证失败!");
            }
            var isOk = await UserRepository.GetAll().SelectMany(p => p.Contacts).AnyAsync(p => p.Mobile.Equals(dto.Mobile, StringComparison.CurrentCultureIgnoreCase));
            if (isOk)
                throw new AbpException("此手机号码已存在联系人中！");
            var user = await UserRepository.GetAll().FirstOrDefaultAsync(p => p.Id == AbpSession.UserId.Value);
            user.Contacts.Add(new Contacts()
            {
                Mobile = dto.Mobile
            });
            await userManager.UpdateAsync(user);
        }

        /// <summary>
        /// 删除联系人(需鉴权【普通用户】)
        /// </summary>
        /// <param name="Id">联系人Id</param>
        /// <returns></returns>
        public async Task RemoveContacts(int Id)
        {
            var user = await UserRepository.GetAll().Include(p => p.Contacts).FirstOrDefaultAsync(p => p.Id == AbpSession.UserId.Value);
            var contact = user.Contacts.FirstOrDefault(p => p.Id == Id);
            if (contact != null)
                user.Contacts.Remove(contact);
            await userManager.UpdateAsync(user);
        }

        #endregion

        #region 我的地址管理
        /// <summary>
        /// 保存地址(需鉴权【普通用户】)
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task SaveAddress(AddAddressDto dto)
        {
            if (!AbpSession.UserId.HasValue)
                throw new AbpException("错误的请求");
            var user = await UserRepository.GetAll().Include(p => p.Address).FirstOrDefaultAsync(p => p.Id == AbpSession.UserId.Value);
            if (dto.IsDefault)
            {
                var address = user.Address.FirstOrDefault(p => p.IsDefault);
                if (address != null) address.IsDefault = false;
            }
            if (dto.Id == default(int))
            {
                user.Address.Add(this.ObjectMapper.Map<Address>(dto));
            }
            else
            {
                var address = user.Address.FirstOrDefault(p => p.Id == dto.Id);
                if (address == null)
                    throw new AbpException("错误的地址id");
                address.City = dto.City;
                address.Area = dto.Area;
                address.Street = dto.Street;
                address.DetailAddress = dto.DetailAddress;
                address.IsDefault = dto.IsDefault;
            }
            await userManager.UpdateAsync(user);
        }

        /// <summary>
        /// 删除地址(需鉴权【普通用户】)
        /// </summary>
        /// <param name="Id">用户地址Id</param>
        /// <returns></returns>
        public async Task RemoveAddress(int Id)
        {
            var user = await UserRepository.GetAll().Include(p => p.Address).FirstOrDefaultAsync(p => p.Id == AbpSession.UserId.Value);
            var address = user.Address.FirstOrDefault(p => p.Id == Id);
            if (address != null)
                user.Address.Remove(address);
            await userManager.UpdateAsync(user);
        }

        #endregion

        #region 我的车辆管理
        /// <summary>
        /// 添加我的车(需鉴权【普通用户】)
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task AddUserCar(AddUserCarDto dto)
        {
            if (!AbpSession.UserId.HasValue)
                throw new AbpException("token无效");
            var user = await userManager.GetUserByIdAsync(AbpSession.UserId.Value);
            if (user == null)
                throw new AbpException("token无效");
            var Car = await CarRepository.FirstOrDefaultAsync(p => p.Id == dto.CarId);
            if (Car == null)
                throw new AbpException("车型Id无效");
            var CarName = Car.Name;
            user.UserCars.Add(new UserCar()
            {
                CarId = dto.CarId,
                CarName = CarName,
                FrameNo = dto.FrameNo,
                Type = dto.Type
            });
            await userManager.UpdateAsync(user);
        }

        /// <summary>
        /// 添加我的车(需鉴权【普通用户】)
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [AbpAuthorize(PermissionNames.Pages_Members)]
        public async Task AddCModelCar(AddUserCModelDto dto)
        {
            if (!AbpSession.UserId.HasValue)
                throw new AbpException("错误的请求");
            var user = await userManager.GetUserByIdAsync(AbpSession.UserId.Value);
            var CModelName = (await CModelRepository.FirstOrDefaultAsync(p => p.Id == dto.CModelId)).Name;
            user.UserCModels.Add(new UserCModel()
            {
                CModelId = dto.CModelId,
                CModelName = CModelName,
                FrameNo = dto.FrameNo
            });
            await userManager.UpdateAsync(user);
        }

        #endregion


        /// <summary>
        /// 获取用户信息(需鉴权【普通用户】)
        /// </summary>
        /// <returns></returns>

        public async Task<UserInfoDto> GetUserInfo()
        {
            if (!AbpSession.UserId.HasValue)
                throw new AbpException("请先登录");
            var user = await userManager.GetUserByIdAsync(AbpSession.UserId.Value);
            user = await UserRepository.GetAll().Include(p => p.Address).Include(p => p.Contacts).Include(p => p.Wechats).Include(p => p.UserCars).Include(p => p.UserCModels).FirstOrDefaultAsync(p => p.Id == AbpSession.UserId.Value);
            UserInfoDto userInfoDto = this.ObjectMapper.Map<UserInfoDto>(user);
            userInfoDto.MyAppointmentCount = (await AppointmentRepository.CountAsync(p => p.UserId == AbpSession.UserId.Value))
                + (await AppointmentWebActivityRepository.CountAsync(p => p.UserId == AbpSession.UserId.Value))
                + (await AppointmentTestDrivRepository.CountAsync(p => p.UserId == AbpSession.UserId.Value));
            //userInfoDto.MyAppointmentActivityCount = await AppointmentActivityRepository.CountAsync(p => p.UserId == AbpSession.UserId.Value);
            userInfoDto.MyOrderCount = await OrderRepository.CountAsync(p => p.UserId == AbpSession.UserId.Value && (p.State != 0 || (p.State == 0 && p.CreationTime.AddMinutes(30) >= DateTime.Now)));
            userInfoDto.MyShoppingCartCount = await ShoppingCartRepository.CountAsync(p => p.UserId == AbpSession.UserId.Value);
            userInfoDto.Mobile = string.IsNullOrWhiteSpace(userInfoDto.Mobile) ? "" : userInfoDto.Mobile;
            return userInfoDto;
        }

        #region  修改我的信息
        /// <summary>
        /// 修改我的信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task SaveUserInfo(UserEditDto input)
        {
            if (!AbpSession.UserId.HasValue)
                throw new AbpException("请先登录");
            try
            {
                User user = UserRepository.GetAll().Where(k => k.Id == AbpSession.UserId.Value).FirstOrDefault();
                user.Surname = input.Surname;
                user.Name = input.Name;
                user.Mobile = input.Mobile;
                user.Sex = input.Sex;
                user.IsCarOwner = input.IsCarOwner;
                user.LastStoreId = input.LastStoreId;
                await UserRepository.UpdateAsync(user);
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Logger.Error("修改我的信息失败！", e);
                throw new AbpException("修改我的信息失败！", e);
            }
        }
        #endregion

    }
    public class AddUserCarDto
    {
        /// <summary>
        /// 车架号
        /// </summary>
        public string FrameNo { get; set; }

        /// <summary>
        /// 车型Id
        /// </summary>
        [Required]
        public int CarId { get; set; }

        /// <summary>
        /// 类型 1保时捷，2阿斯顿马丁
        /// </summary>
        public string Type { get; set; }

    }

    public class AddUserCModelDto
    {
        /// <summary>
        /// 车架号
        /// </summary>
        public string FrameNo { get; set; }

        /// <summary>
        /// 车型Id
        /// </summary>
        [Required]
        public int CModelId { get; set; }

    }


    public class UserEditDto
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
    public class UserInfoDto
    {
        public string UnionId { get; set; }
        /// <summary>
        /// 用户名称
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 姓
        /// </summary>
        public string Surname { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 全名
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// 最后登录时间
        /// </summary>
        public DateTime? LastLoginTime { get; set; }

        /// <summary>
        /// 注册时间
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// 最后一次门店Id
        /// </summary>
        public int LastStoreId { get; set; }

        /// <summary>
        /// 微信信息
        /// </summary>
        public List<Wechat> Wechats { get; set; }

        /// <summary>
        /// 用户的车
        /// </summary>
        public List<UserCarDto> UserCars { get; set; }

        /// <summary>
        /// 用户的车
        /// </summary>
        public List<UserCModelDto> UserCModels { get; set; }

        /// <summary>
        /// 联系人
        /// </summary>
        public List<ContactsDto> Contacts { get; set; }

        /// <summary>
        /// 我的地址
        /// </summary>
        public List<AddressDto> Address { get; set; }
        /// <summary>
        /// 我的预约试驾数量
        /// </summary>
        public int MyAppointmentCount { get; set; }

        /// <summary>
        /// 我的订单数量
        /// </summary>
        public int MyOrderCount { get; set; }

        /// <summary>
        /// 我的购物车数量
        /// </summary>
        public int MyShoppingCartCount { get; set; }
    }

    public class ContactsDto : EntityDto
    {
        /// <summary>
        /// 联系人名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 联系人手机号码
        /// </summary>

        public string Mobile { get; set; }
    }

    public class WechatDto
    {
        /// <summary>
        /// OpenId
        /// </summary>
        public string OpenId { get; set; }

        /// <summary>
        /// 微信昵称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string Avater { get; set; }

        /// <summary>
        /// 0 Web 1公众号 2小程序
        /// </summary>
        public int Type { get; set; }
    }
    public class UserCarDto
    {
        /// <summary>
        /// 我的车辆Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 车型Id
        /// </summary>
        public int CarId { get; set; }

        /// <summary>
        /// 车架号
        /// </summary>
        public string FrameNo { get; set; }

        /// <summary>
        /// 车型名称
        /// </summary>
        public string CarName { get; set; }

        /// <summary>
        /// 车类型
        /// </summary>
        public string Type { get; set; }
    }

    public class UserCModelDto
    {
        /// <summary>
        /// 我的车辆Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 车型Id
        /// </summary>
        public int CModelId { get; set; }

        /// <summary>
        /// 车架号
        /// </summary>
        public string FrameNo { get; set; }

        /// <summary>
        /// 车型名称
        /// </summary>
        public string CModelName { get; set; }
    }

    public class AddContactsDto
    {
        /// <summary>
        /// 联系人手机号码
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Mobile { get; set; }
        ///// <summary>
        ///// 联系人名称
        ///// </summary>
        //[Required]
        //[StringLength(10)]
        //public string Name { get; set; }

        /// <summary>
        /// 短信验证码
        /// </summary>
        [Required]
        [StringLength(6)]
        public string Code { get; set; }
    }

    public class AddAddressDto : EntityDto
    {
        /// <summary>
        /// 城市
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// 地区
        /// </summary>
        public string Area { get; set; }

        /// <summary>
        /// 街道
        /// </summary>
        public string Street { get; set; }

        /// <summary>
        /// 详细地址
        /// </summary>
        public string DetailAddress { get; set; }

        /// <summary>
        /// 是否默认地址
        /// </summary>
        public Boolean IsDefault { get; set; }
    }

    public class AddressDto : EntityDto
    {
        public long UserId { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// 地区
        /// </summary>
        public string Area { get; set; }

        /// <summary>
        /// 街道
        /// </summary>
        public string Street { get; set; }

        /// <summary>
        /// 详细地址
        /// </summary>
        public string DetailAddress { get; set; }

        /// <summary>
        /// 是否默认地址
        /// </summary>
        public Boolean IsDefault { get; set; }
    }
}

