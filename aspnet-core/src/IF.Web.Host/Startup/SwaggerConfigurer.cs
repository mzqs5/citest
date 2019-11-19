using IF.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace IF
{
    public static class SwaggerConfigurer
    {
        /// <summary>
        /// 配置Swagger接口文档
        /// </summary>
        /// <param name="services"></param>
        public static void UseMySwagger(this IServiceCollection services)
        {
            // Swagger - Enable this line and the related lines in Configure method to enable swagger UI
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info { Title = "IF API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);

                // Define the BearerAuth scheme that's in use
                options.AddSecurityDefinition("bearerAuth", new ApiKeyScheme()
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });
                try
                {
                    IEnumerable<string> enumerable = Directory.EnumerateFiles(Path.Combine(AppContext.BaseDirectory, "apidocs"), "*.xml", SearchOption.TopDirectoryOnly);
                    if (enumerable != null)
                    {
                        foreach (string str2 in enumerable)
                        {
                            options.IncludeXmlComments(str2, false);
                        }
                    }
                }
                catch (Exception)
                {

                }
            });
        }

        public static void UseMySwagger(this IApplicationBuilder app)
        {
            // Enable middleware to serve generated Swagger as a JSON endpoint
            var virtualPath = AppConfigurations.GetAppSettings().GetSection("virtualPath").Value;

            app.UseSwagger(c =>
            {
                c.PreSerializeFilters.Add((swaggerDoc, httpReq) => swaggerDoc.BasePath = virtualPath);
            });
            // Enable middleware to serve swagger-ui assets (HTML, JS, CSS etc.)
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint(virtualPath + "/swagger/v1/swagger.json", "IF API V1");
                options.IndexStream = () => Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("IF.Web.Host.wwwroot.swagger.ui.index.html");
            }); // URL: /swagger
        }
    }
}
