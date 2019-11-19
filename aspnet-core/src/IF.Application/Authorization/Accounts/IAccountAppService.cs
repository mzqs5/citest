using System.Threading.Tasks;
using Abp.Application.Services;
using IF.Authorization.Accounts.Dto;

namespace IF.Authorization.Accounts
{
    public interface IAccountAppService : IApplicationService
    {

        Task<RegisterOutput> Register(RegisterInput input);
    }
}
