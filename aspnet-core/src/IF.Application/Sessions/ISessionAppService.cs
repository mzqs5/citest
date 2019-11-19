using System.Threading.Tasks;
using Abp.Application.Services;
using IF.Sessions.Dto;

namespace IF.Sessions
{
    public interface ISessionAppService : IApplicationService
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
    }
}
