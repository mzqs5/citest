using Abp;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using IF.Authorization;
using IF.Authorization.Roles;
using IF.Authorization.Users;
using IF.Roles.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IF
{
    public class AdminRoleAppService : IFAppServiceBase
    {
        private readonly RoleManager _roleManager;
        private readonly UserManager _userManager;
        IRepository<User, long> UserRepository;
        public AdminRoleAppService(RoleManager _roleManager,
            UserManager _userManager,
            IRepository<User, long> UserRepository)
        {
            this._roleManager = _roleManager;
            this._userManager = _userManager;
            this.UserRepository = UserRepository;
        }
        private IEnumerable<RoleListDto> GetDataSource()
        {
            var roles = _roleManager.Roles.Where(p => !p.IsStatic).AsEnumerable();
            var result = from role in roles
                         select new RoleListDto
                         {
                             Id = role.Id,
                             Name = role.Name,
                             DisplayName = role.DisplayName,
                             Description = role.Description,
                             IsDefault = role.IsDefault,
                             IsStatic = role.IsStatic,
                             CreationTime = role.CreationTime
                         };
            return result;
        }

        #region 获取角色列表
        /// <summary>
        /// 获取角色列表，支持分页查询
        /// </summary>
        /// <param name="loadOptions">查询参数</param>
        /// <returns></returns>
        public async Task<object> GetListDataAsync(DataSourceLoadOptions loadOptions)
        {
            loadOptions.Sort = new SortingInfo[] { new SortingInfo() { Selector = "CreationTime", Desc = true } };
            return base.DataSourceLoadMap(GetDataSource(), loadOptions);
        }
        #endregion

        #region 批量删除角色
        /// <summary>
        /// 批量删除角色
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete]
        [AbpAuthorize("Admin.Roles.Delete")]
        public async Task BatchDeleteAsync(string ids)
        {
            try
            {
                foreach (var id in ids.Split(','))
                {
                    var role = await _roleManager.FindByIdAsync(id);
                    var users = await _userManager.GetUsersInRoleAsync(role.NormalizedName);

                    foreach (var user in users)
                    {
                        CheckErrors(await _userManager.RemoveFromRoleAsync(user, role.NormalizedName));
                    }

                    CheckErrors(await _roleManager.DeleteAsync(role));
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Logger.Error("角色删除异常！", e);
                throw new AbpException("角色删除异常！", e);
            }
        }
        #endregion


        public async Task<IList<string>> GetMenuPermissions()
        {
            var user = UserRepository.GetAll().Include(p => p.Roles).FirstOrDefault(p => p.Id == AbpSession.UserId.Value);
            List<string> list = new List<string>();
            user.Roles.ToList().ForEach(role =>
            {
                var permissions = _roleManager.GetGrantedPermissionsAsync(role.RoleId).Result;
                list.AddRange(permissions.Select(p => p.Name).Where(p => p.Contains("Pages.")));
            });
            return list;
        }

        public async Task<IList<string>> GetOperationPermissions()
        {
            var user = UserRepository.GetAll().Include(p => p.Roles).Include(p => p.Permissions).FirstOrDefault(p => p.Id == AbpSession.UserId.Value);
            List<string> list = new List<string>();
            user.Roles.ToList().ForEach(role =>
            {
                var permissions = _roleManager.GetGrantedPermissionsAsync(role.RoleId).Result;
                list.AddRange(permissions.Select(p => p.Name).Where(p => p.Contains("Admin.")));
            });
            return list;
        }
        [HttpGet]
        public async Task<int> IsGranted(string permissionName)
        {
            var user = UserRepository.GetAll().Include(p => p.Roles).Include(p => p.Permissions).FirstOrDefault(p => p.Id == AbpSession.UserId.Value);
            List<string> list = new List<string>();
            user.Roles.ToList().ForEach(role =>
            {
                var permissions = _roleManager.GetGrantedPermissionsAsync(role.RoleId).Result;
                list.AddRange(permissions.Select(p => p.Name).Where(p => p.Equals(permissionName, StringComparison.CurrentCultureIgnoreCase)));
            });
            return list.Count > 0 ? 1 : 0;
        }
    }
}