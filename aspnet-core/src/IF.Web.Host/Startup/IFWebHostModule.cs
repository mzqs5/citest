using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using IF.Configuration;
using Abp.Web.Models;
using Abp.AspNetCore.Configuration;

namespace IF.Web.Host.Startup
{
    [DependsOn(
       typeof(IFWebCoreModule))]
    public class IFWebHostModule : AbpModule
    {
        private readonly IHostingEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public IFWebHostModule(IHostingEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void Initialize()
        {
            var result = new DontWrapResultAttribute();
            Configuration.Modules.AbpAspNetCore().DefaultWrapResultAttribute.WrapOnError = result.WrapOnError;
            Configuration.Modules.AbpAspNetCore().DefaultWrapResultAttribute.WrapOnSuccess = result.WrapOnSuccess;

            IocManager.RegisterAssemblyByConvention(typeof(IFWebHostModule).GetAssembly());
        }
    }
}
