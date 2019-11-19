using Abp.AspNetCore;
using Abp.Castle.Logging.Log4Net;
using Abp.Hangfire;
using Castle.Facilities.Logging;
using Essensoft.AspNetCore.Payment.WeChatPay;
using Hangfire;
using Hangfire.SqlServer;
using IF.Configuration;
using IF.Filter;
using IF.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace IF.Web.Host.Startup
{
    public class Startup
    {
        private const string _defaultCorsPolicyName = "AllowAll";

        private readonly IConfigurationRoot _appConfiguration;
        public IConfiguration Configuration { get; }

        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            Configuration = configuration;
            _appConfiguration = env.GetAppConfiguration();
            string filePath = env.WebRootPath + $@"\files\";
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // MVC
            services.AddMvc(
                options =>
                {
                    options.Filters.Add(new CorsAuthorizationFilterFactory(_defaultCorsPolicyName));
                    options.Filters.AddService(typeof(ExceptionFilter), order: 0);
                }
            ).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            IdentityRegistrar.Register(services);
            AuthConfigurer.Configure(services, _appConfiguration);

            //services.AddSignalR();

            services.AddHangfire(x => x.UseStorage(new SqlServerStorage(_appConfiguration.GetConnectionString("Default"))));

            // Configure CORS for angular2 UI
            services.AddCors(options =>
                    options.AddPolicy(_defaultCorsPolicyName, p => p.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader()));

            services.UseMySwagger();

            services.AddWeChatPay();
            services.Configure<WeChatPayOptions>(Configuration.GetSection("WeChatPay"));

            services.AddScoped<IHttpContextAccessor, HttpContextAccessor>();

            // Configure Abp and Dependency Injection
            return services.AddAbp<IFWebHostModule>(
                // Configure Log4Net logging
                options => options.IocManager.IocContainer.AddFacility<LoggingFacility>(
                    f => f.UseAbpLog4Net().WithConfig("log4net.config")
                )
            );
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseAbp(options => { options.UseAbpRequestLocalization = false; }); // Initializes ABP framework.

            app.UseCors(_defaultCorsPolicyName); // Enable CORS!

            app.UseStaticFiles();

            app.UseAuthentication();

            //app.UseAbpRequestLocalization();

            //app.UseSignalR(routes =>
            //{
            //    routes.MapHub<AbpCommonHub>("/signalr");
            //});

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "defaultWithArea",
                    template: "{area}/{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseMySwagger();

            app.UseHangfireServer();
            app.UseHangfireDashboard();
            var virtualPath = AppConfigurations.GetAppSettings().GetSection("virtualPath").Value;
            app.UseHangfireDashboard(virtualPath + "/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizeFilter() }
            });
        }
    }
}
