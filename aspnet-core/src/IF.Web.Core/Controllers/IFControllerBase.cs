using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace IF.Controllers
{
    public abstract class IFControllerBase: AbpController
    {
        protected IFControllerBase()
        {
            LocalizationSourceName = IFConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
