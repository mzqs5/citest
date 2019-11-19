using Abp.EntityFrameworkCore;
using Abp.EntityFrameworkCore.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Zero.EntityFrameworkCore;
using IF.EntityFrameworkCore.Seed;
using Abp.Configuration.Startup;
using Abp.Domain.Uow;

namespace IF.EntityFrameworkCore
{
    [DependsOn(
        typeof(IFCoreModule), 
        typeof(AbpZeroCoreEntityFrameworkCoreModule))]
    public class IFEntityFrameworkModule : AbpModule
    {
        /* Used it tests to skip dbcontext registration, in order to use in-memory database of EF Core */
        public bool SkipDbContextRegistration { get; set; }

        public bool SkipDbSeed { get; set; }

        public override void PreInitialize()
        {
            if (!SkipDbContextRegistration)
            {
                Configuration.Modules.AbpEfCore().AddDbContext<IFDbContext>(options =>
                {
                    if (options.ExistingConnection != null)
                    {
                        IFDbContextConfigurer.Configure(options.DbContextOptions, options.ExistingConnection);
                    }
                    else
                    {
                        IFDbContextConfigurer.Configure(options.DbContextOptions, options.ConnectionString);
                    }
                });
            }
            //base.PreInitialize();
            //Configuration.ReplaceService<IConnectionStringResolver, MyConnectionStringResolver>();
            //this.AddDbContext<IFDbContext>();//数据库一
            //this.AddDbContext<OutSideDbContext>();//数据库二
        }

        private void AddDbContext<TDbContext>()
            where TDbContext : AbpDbContext
        {
            Configuration.Modules.AbpEfCore().AddDbContext<TDbContext>(options =>
            {
                if (options.ExistingConnection != null)
                {
                    IFDbContextConfigurer.Configure(options.DbContextOptions, options.ExistingConnection);
                }
                else
                {
                    IFDbContextConfigurer.Configure(options.DbContextOptions, options.ConnectionString);
                }
            });
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(IFEntityFrameworkModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            if (!SkipDbSeed)
            {
                SeedHelper.SeedHostDb(IocManager);
            }
        }
    }
}
