using Abp.AutoMapper;
using Abp.Hangfire;
using Abp.Hangfire.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Threading.BackgroundWorkers;
using Hangfire;
using IF.Authorization;
using IF.Porsche;

namespace IF
{
    [DependsOn(
        typeof(IFCoreModule),
        typeof(AbpAutoMapperModule))]
    public class IFApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Authorization.Providers.Add<IFAuthorizationProvider>();
            //Configuration.BackgroundJobs.UseHangfire(configuration =>
            //{
            //    configuration.GlobalConfiguration.UseSqlServerStorage("Default");
            //});
        }

        public override void Initialize()
        {
            var thisAssembly = typeof(IFApplicationModule).GetAssembly();
            IocManager.RegisterAssemblyByConvention(thisAssembly);

            Configuration.Modules.AbpAutoMapper().Configurators.Add(
                // Scan the assembly for classes which inherit from AutoMapper.Profile
                cfg => cfg.AddProfile<ApplicationAutoMapperProfile>()
            );
        }

        public override void PostInitialize()
        {
            base.PostInitialize();

        }
    }
}
