using System.Threading.Tasks;
using IF.Configuration.Dto;

namespace IF.Configuration
{
    public interface IConfigurationAppService
    {
        Task ChangeUiTheme(ChangeUiThemeInput input);
    }
}
