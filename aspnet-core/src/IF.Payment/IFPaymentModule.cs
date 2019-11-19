using Abp.Modules;
using Abp.Reflection.Extensions;
using Abp.Zero;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace IF.Payment
{
    [DependsOn(typeof(AbpZeroCoreModule))]
    public class IFPaymentModule : AbpModule
    {
        public override void PreInitialize()
        {
            //Configuration.Auditing.IsEnabledForAnonymousUsers = true;
            Configuration.IocManager.Register<IHttpContextAccessor, HttpContextAccessor>();
            Configuration.IocManager.Register<IHttpContextAccessor, HttpContextAccessor>();
            services.AddWeChatPay();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(IFPaymentModule).GetAssembly());
        }

        public override void PostInitialize()
        {
            //IocManager.Resolve<AppTimes>().StartupTime = Clock.Now;
        }
    }
}
