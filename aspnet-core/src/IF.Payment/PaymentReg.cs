using Essensoft.AspNetCore.Payment.WeChatPay;
using IF.Payment.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace IF.Payment
{
    public static class PaymentReg
    {
        public static void RegPayment(this IServiceCollection services)
        {
            services.AddWeChatPay();
            var config = AppConfigurations.GetAppSettings().GetSection("WeChatPay").Value;
            //JsonConvert.DeserializeObject<WeChatPayOptions>(config)
            services.Configure<WeChatPayOptions>(Configuration.GetSection("WeChatPay"));
        }
    }
}
