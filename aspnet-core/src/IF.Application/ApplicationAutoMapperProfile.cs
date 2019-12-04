using Abp.Authorization;
using Abp.Authorization.Roles;
using AutoMapper;
using IF.Authorization;
using IF.Authorization.Accounts;
using IF.Authorization.Roles;
using IF.Authorization.Users;
using IF.Common;
using IF.Porsche;
using IF.Roles.Dto;
using IF.Users.Dto;
using System.Linq;
using System.Reflection;

namespace IF
{
    public class ApplicationAutoMapperProfile : Profile
    {
        public ApplicationAutoMapperProfile()
        {

            #region 自动映射 规则 Dto EditDto ListDto
            var applicationContractsModule = Assembly.GetAssembly(typeof(IFApplicationModule)).GetTypes();
            var domainModule = Assembly.GetAssembly(typeof(IFCoreModule)).GetTypes();
            var aggregateRootList = domainModule.Where(t => t.BaseType == typeof(AggregateRootBase<int>));
            foreach (var sourceType in aggregateRootList)
            {
                var sourceTypeName = sourceType.Name.Replace("Aggregate", "");
                var Dto = applicationContractsModule.Where(t => t.Name.Equals(sourceTypeName + "Dto")).FirstOrDefault();
                if (Dto != null)
                    this.CreateMap(sourceType, Dto);
                var ListDto = applicationContractsModule.Where(t => t.Name.Equals(sourceTypeName + "ListDto")).FirstOrDefault();
                if (ListDto != null)
                    this.CreateMap(sourceType, ListDto);

                var EditDto = applicationContractsModule.Where(t => t.Name.Equals(sourceTypeName + "EditDto")).FirstOrDefault();
                if (EditDto != null)
                    this.CreateMap(EditDto, sourceType);
            }

            var EntityList = domainModule.Where(t => t.BaseType == typeof(EntityBase<int>));
            foreach (var sourceType in EntityList)
            {
                var Dto = applicationContractsModule.Where(t => t.Name.Equals(sourceType.Name + "Dto")).FirstOrDefault();
                if (Dto != null)
                    this.CreateMap(sourceType, Dto);
                var EditDto = applicationContractsModule.Where(t => t.Name.Equals(sourceType.Name + "EditDto")).FirstOrDefault();
                if (EditDto != null)
                    this.CreateMap(EditDto, sourceType);
            }
            #endregion

            #region 特殊映射
            CreateMap<UserDto, User>();
            CreateMap<UserDto, User>()
                .ForMember(x => x.Roles, opt => opt.Ignore())
                .ForMember(x => x.CreationTime, opt => opt.Ignore());

            CreateMap<User, UserInfoDto>();
            CreateMap<Contacts, ContactsDto>();
            CreateMap<Address, AddressDto>();
            CreateMap<AddAddressDto, Address>();
            CreateMap<AdminUserEditDto, User>();
            CreateMap<User, AdminUserDto>();
            CreateMap<UserEditDto, User>();
            CreateMap<UserAccountEditDto, User>();
            CreateMap<User, UserAccountDto>();
            CreateMap<UserCar, UserCarDto>();
            CreateMap<UserCModel, UserCModelDto>();
            CreateMap<Wechat, WechatDto>();

            CreateMap<CreateUserDto, User>();
            CreateMap<CreateUserDto, User>().ForMember(x => x.Roles, opt => opt.Ignore());

            CreateMap<Permission, string>().ConvertUsing(r => r.Name);
            CreateMap<RolePermissionSetting, string>().ConvertUsing(r => r.Name);

            CreateMap<CreateRoleDto, Role>();

            CreateMap<RoleDto, Role>();

            CreateMap<Role, RoleDto>().ForMember(x => x.GrantedPermissions,
                opt => opt.MapFrom(x => x.Permissions.Where(p => p.IsGranted)));

            CreateMap<Role, RoleListDto>();
            CreateMap<Role, RoleEditDto>();
            CreateMap<Permission, FlatPermissionDto>();

            CreateMap<AppointmentDto, AllAppointmentDto>();
            CreateMap<AppointmentWebActivityDto, AllAppointmentDto>();
            CreateMap<AppointmentTestDriveDto, AllAppointmentDto>();
            CreateMap<AppointmentEuroDriveDto, AllAppointmentDto>();
            CreateMap<AdminAppointmentActivityEditDto, AppointmentActivityAggregate>();
            #endregion

        }
    }
}
