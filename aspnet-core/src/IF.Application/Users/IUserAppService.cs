using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using IF.Roles.Dto;
using IF.Users.Dto;

namespace IF.Users
{
    public interface IUserAppService : IAsyncCrudAppService<UserDto, long, PagedUserResultRequestDto, CreateUserDto, UserDto>
    {
        Task<ListResultDto<RoleDto>> GetRoles();

        Task ChangeLanguage(ChangeUserLanguageDto input);
    }
}
