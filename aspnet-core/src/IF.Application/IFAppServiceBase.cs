using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Abp.Application.Services;
using Abp.IdentityFramework;
using Abp.Runtime.Session;
using IF.Authorization.Users;
using IF.MultiTenancy;
using DevExtreme.AspNet.Data.ResponseModel;
using IF.Common;
using System.Collections.Generic;
using DevExtreme.AspNet.Data;
using System.Linq;

namespace IF
{
    /// <summary>
    /// Derive your application services from this class.
    /// </summary>
    public abstract class IFAppServiceBase : ApplicationService
    {
        public TenantManager TenantManager { get; set; }

        public UserManager UserManager { get; set; }

        protected IFAppServiceBase()
        {
            LocalizationSourceName = IFConsts.LocalizationSourceName;
        }

        protected virtual async Task<User> GetCurrentUserAsync()
        {
            var user = await UserManager.FindByIdAsync(AbpSession.GetUserId().ToString());
            if (user == null)
            {
                throw new Exception("There is no current user!");
            }

            return user;
        }

        protected virtual Task<Tenant> GetCurrentTenantAsync()
        {
            return TenantManager.GetByIdAsync(AbpSession.GetTenantId());
        }

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }

        private object ConvertDataSource<T>(LoadResult result)
        {
            string str = JsonHelper.SerializeObjectWithCamelCasePropertyNames(result);
            return JsonHelper.ConvertToObject<DataSourceLoadResult<T>>(result);
        }

        [Obsolete("使用 DataSourceLoadAsync 代替")]
        public object DataSourceLoad<T>(IEnumerable<T> source, DataSourceLoadOptionsBase options)
        {
            return this.ConvertDataSource<T>(DataSourceLoader.Load<T>(source, options));
        }

        [Obsolete("使用DataSourceLoadAsync代替")]
        public object DataSourceLoad<T>(IQueryable<T> source, DataSourceLoadOptionsBase options)
        {
            return this.ConvertDataSource<T>(DataSourceLoader.Load<T>(source, options));
        }

        public DataSourceLoadResult<T> DataSourceLoadMap<T>(IEnumerable<T> source, DataSourceLoadOptionsBase options)
        {
            return JsonHelper.ConvertToObject<DataSourceLoadResult<T>>(this.DataSourceLoad<T>(source, options));
        }

        public DataSourceLoadResult<T> DataSourceLoadMap<T>(IQueryable<T> source, DataSourceLoadOptionsBase options)
        {
            return JsonHelper.ConvertToObject<DataSourceLoadResult<T>>(this.DataSourceLoad<T>(source, options));
        }
    }
}
