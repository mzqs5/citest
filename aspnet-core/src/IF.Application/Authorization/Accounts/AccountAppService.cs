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
    /// �����Ȩ
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
        /// �û�ע�ᣨ�����Ȩ��
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
                Logger.Error("������֤ʧ�ܣ�");
                throw new AbpException("������֤ʧ��!");
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

        #region ��ϵ�˹���
        /// <summary>
        /// �����ϵ��(���Ȩ����ͨ�û���)
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task AddContacts(AddContactsDto dto)
        {
            if (!AbpSession.UserId.HasValue)
                throw new AbpException("���������");
            var Sms = await SmsRepository.GetAll().Where(k => k.Mobile == dto.Mobile
                    && k.Code == dto.Code
                    && k.CreationTime.AddMinutes((Double)k.EffectiveTime) >= DateTime.Now)
                    .FirstOrDefaultAsync();
            if (Sms == null)
            {
                Logger.Error("������֤ʧ�ܣ�");
                throw new AbpException("������֤ʧ��!");
            }
            var isOk = await UserRepository.GetAll().SelectMany(p => p.Contacts).AnyAsync(p => p.Mobile.Equals(dto.Mobile, StringComparison.CurrentCultureIgnoreCase));
            if (isOk)
                throw new AbpException("���ֻ������Ѵ�����ϵ���У�");
            var user = await UserRepository.GetAll().FirstOrDefaultAsync(p => p.Id == AbpSession.UserId.Value);
            user.Contacts.Add(new Contacts()
            {
                Mobile = dto.Mobile
            });
            await userManager.UpdateAsync(user);
        }

        /// <summary>
        /// ɾ����ϵ��(���Ȩ����ͨ�û���)
        /// </summary>
        /// <param name="Id">��ϵ��Id</param>
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

        #region �ҵĵ�ַ����
        /// <summary>
        /// �����ַ(���Ȩ����ͨ�û���)
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task SaveAddress(AddAddressDto dto)
        {
            if (!AbpSession.UserId.HasValue)
                throw new AbpException("���������");
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
                    throw new AbpException("����ĵ�ַid");
                address.City = dto.City;
                address.Area = dto.Area;
                address.Street = dto.Street;
                address.DetailAddress = dto.DetailAddress;
                address.IsDefault = dto.IsDefault;
            }
            await userManager.UpdateAsync(user);
        }

        /// <summary>
        /// ɾ����ַ(���Ȩ����ͨ�û���)
        /// </summary>
        /// <param name="Id">�û���ַId</param>
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

        #region �ҵĳ�������
        /// <summary>
        /// ����ҵĳ�(���Ȩ����ͨ�û���)
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public async Task AddUserCar(AddUserCarDto dto)
        {
            if (!AbpSession.UserId.HasValue)
                throw new AbpException("token��Ч");
            var user = await userManager.GetUserByIdAsync(AbpSession.UserId.Value);
            if (user == null)
                throw new AbpException("token��Ч");
            var Car = await CarRepository.FirstOrDefaultAsync(p => p.Id == dto.CarId);
            if (Car == null)
                throw new AbpException("����Id��Ч");
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
        /// ����ҵĳ�(���Ȩ����ͨ�û���)
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [AbpAuthorize(PermissionNames.Pages_Members)]
        public async Task AddCModelCar(AddUserCModelDto dto)
        {
            if (!AbpSession.UserId.HasValue)
                throw new AbpException("���������");
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
        /// ��ȡ�û���Ϣ(���Ȩ����ͨ�û���)
        /// </summary>
        /// <returns></returns>

        public async Task<UserInfoDto> GetUserInfo()
        {
            if (!AbpSession.UserId.HasValue)
                throw new AbpException("���ȵ�¼");
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

        #region  �޸��ҵ���Ϣ
        /// <summary>
        /// �޸��ҵ���Ϣ
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task SaveUserInfo(UserEditDto input)
        {
            if (!AbpSession.UserId.HasValue)
                throw new AbpException("���ȵ�¼");
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
                Logger.Error("�޸��ҵ���Ϣʧ�ܣ�", e);
                throw new AbpException("�޸��ҵ���Ϣʧ�ܣ�", e);
            }
        }
        #endregion

    }
    public class AddUserCarDto
    {
        /// <summary>
        /// ���ܺ�
        /// </summary>
        public string FrameNo { get; set; }

        /// <summary>
        /// ����Id
        /// </summary>
        [Required]
        public int CarId { get; set; }

        /// <summary>
        /// ���� 1��ʱ�ݣ�2��˹����
        /// </summary>
        public string Type { get; set; }

    }

    public class AddUserCModelDto
    {
        /// <summary>
        /// ���ܺ�
        /// </summary>
        public string FrameNo { get; set; }

        /// <summary>
        /// ����Id
        /// </summary>
        [Required]
        public int CModelId { get; set; }

    }


    public class UserEditDto
    {
        /// <summary>
        /// ��
        /// </summary>
        public new string Name { get; set; }
        /// <summary>
        /// ��
        /// </summary>
        public new string Surname { get; set; }

        /// <summary>
        /// �Ա� 0�� 1Ů
        /// </summary>
        public int Sex { get; set; }
        /// <summary>
        /// �Ƿ��� 0�� 1��
        /// </summary>
        public int IsCarOwner { get; set; }
        /// <summary>
        /// �ֻ���
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// �����ŵ�
        /// </summary>
        public int LastStoreId { get; set; }
    }
    public class UserInfoDto
    {
        public string UnionId { get; set; }
        /// <summary>
        /// �û�����
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// ��
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ��
        /// </summary>
        public string Surname { get; set; }

        /// <summary>
        /// �ֻ�����
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// ȫ��
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// ����¼ʱ��
        /// </summary>
        public DateTime? LastLoginTime { get; set; }

        /// <summary>
        /// ע��ʱ��
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// ���һ���ŵ�Id
        /// </summary>
        public int LastStoreId { get; set; }

        /// <summary>
        /// ΢����Ϣ
        /// </summary>
        public List<Wechat> Wechats { get; set; }

        /// <summary>
        /// �û��ĳ�
        /// </summary>
        public List<UserCarDto> UserCars { get; set; }

        /// <summary>
        /// �û��ĳ�
        /// </summary>
        public List<UserCModelDto> UserCModels { get; set; }

        /// <summary>
        /// ��ϵ��
        /// </summary>
        public List<ContactsDto> Contacts { get; set; }

        /// <summary>
        /// �ҵĵ�ַ
        /// </summary>
        public List<AddressDto> Address { get; set; }
        /// <summary>
        /// �ҵ�ԤԼ�Լ�����
        /// </summary>
        public int MyAppointmentCount { get; set; }

        /// <summary>
        /// �ҵĶ�������
        /// </summary>
        public int MyOrderCount { get; set; }

        /// <summary>
        /// �ҵĹ��ﳵ����
        /// </summary>
        public int MyShoppingCartCount { get; set; }
    }

    public class ContactsDto : EntityDto
    {
        /// <summary>
        /// ��ϵ������
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ��ϵ���ֻ�����
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
        /// ΢���ǳ�
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// ͷ��
        /// </summary>
        public string Avater { get; set; }

        /// <summary>
        /// 0 Web 1���ں� 2С����
        /// </summary>
        public int Type { get; set; }
    }
    public class UserCarDto
    {
        /// <summary>
        /// �ҵĳ���Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// ����Id
        /// </summary>
        public int CarId { get; set; }

        /// <summary>
        /// ���ܺ�
        /// </summary>
        public string FrameNo { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        public string CarName { get; set; }

        /// <summary>
        /// ������
        /// </summary>
        public string Type { get; set; }
    }

    public class UserCModelDto
    {
        /// <summary>
        /// �ҵĳ���Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// ����Id
        /// </summary>
        public int CModelId { get; set; }

        /// <summary>
        /// ���ܺ�
        /// </summary>
        public string FrameNo { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        public string CModelName { get; set; }
    }

    public class AddContactsDto
    {
        /// <summary>
        /// ��ϵ���ֻ�����
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Mobile { get; set; }
        ///// <summary>
        ///// ��ϵ������
        ///// </summary>
        //[Required]
        //[StringLength(10)]
        //public string Name { get; set; }

        /// <summary>
        /// ������֤��
        /// </summary>
        [Required]
        [StringLength(6)]
        public string Code { get; set; }
    }

    public class AddAddressDto : EntityDto
    {
        /// <summary>
        /// ����
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string Area { get; set; }

        /// <summary>
        /// �ֵ�
        /// </summary>
        public string Street { get; set; }

        /// <summary>
        /// ��ϸ��ַ
        /// </summary>
        public string DetailAddress { get; set; }

        /// <summary>
        /// �Ƿ�Ĭ�ϵ�ַ
        /// </summary>
        public Boolean IsDefault { get; set; }
    }

    public class AddressDto : EntityDto
    {
        public long UserId { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string Area { get; set; }

        /// <summary>
        /// �ֵ�
        /// </summary>
        public string Street { get; set; }

        /// <summary>
        /// ��ϸ��ַ
        /// </summary>
        public string DetailAddress { get; set; }

        /// <summary>
        /// �Ƿ�Ĭ�ϵ�ַ
        /// </summary>
        public Boolean IsDefault { get; set; }
    }
}

