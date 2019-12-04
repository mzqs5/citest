using System;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Abp.AspNetCore;
using Abp.AspNetCore.Configuration;
using Abp.AspNetCore.SignalR;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Zero.Configuration;
using IF.Authentication.JwtBearer;
using IF.Configuration;
using IF.EntityFrameworkCore;

namespace IF
{
    [DependsOn(
         typeof(IFApplicationModule),
         typeof(IFEntityFrameworkModule),
         typeof(AbpAspNetCoreModule)
     )]
    public class IFWebCoreModule : AbpModule
    {
        private readonly IHostingEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public IFWebCoreModule(IHostingEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void PreInitialize()
        {
            Configuration.DefaultNameOrConnectionString = _appConfiguration.GetConnectionString(
                IFConsts.ConnectionStringName
            );

            // Use database for language management
            //Configuration.Modules.Zero().LanguageManagement.EnableDbLocalization();
            Configuration.Modules.AbpAspNetCore()
                 .CreateControllersForAppServices(
                     typeof(IFApplicationModule).GetAssembly()
                 ).ConfigureControllerModel(model =>
                 {
                     //model.ApiExplorer = "";
                 });

            ConfigureTokenAuth();
        }

        private void ConfigureTokenAuth()
        {
            IocManager.Register<TokenAuthConfiguration>();
            var tokenAuthConfig = IocManager.Resolve<TokenAuthConfiguration>();

            tokenAuthConfig.SecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appConfiguration["Authentication:JwtBearer:SecurityKey"]));
            tokenAuthConfig.Issuer = _appConfiguration["Authentication:JwtBearer:Issuer"];
            tokenAuthConfig.Audience = _appConfiguration["Authentication:JwtBearer:Audience"];
            tokenAuthConfig.SigningCredentials = new SigningCredentials(tokenAuthConfig.SecurityKey, SecurityAlgorithms.HmacSha256);
            tokenAuthConfig.Expiration = TimeSpan.FromDays(365 / 12);
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(IFWebCoreModule).GetAssembly());
        }
    }
}
