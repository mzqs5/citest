using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Abp.Authorization;
using Abp.Authorization.Roles;
using Abp.Authorization.Users;
using Abp.MultiTenancy;
using IF.Authorization;
using IF.Authorization.Roles;
using IF.Authorization.Users;

namespace IF.EntityFrameworkCore.Seed.Tenants
{
    public class TenantRoleAndUserBuilder
    {
        private readonly IFDbContext _context;
        private readonly int _tenantId;

        public TenantRoleAndUserBuilder(IFDbContext context, int tenantId)
        {
            _context = context;
            _tenantId = tenantId;
        }

        public void Create()
        {
            CreateRolesAndUsers();
        }

        private void CreateRolesAndUsers()
        {
            // Admin role

            var adminRole = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == _tenantId && r.Name == StaticRoleNames.Tenants.Admin);
            if (adminRole == null)
            {
                adminRole = _context.Roles.Add(new Role(_tenantId, StaticRoleNames.Tenants.Admin, StaticRoleNames.Tenants.Admin) { IsStatic = true }).Entity;
                _context.SaveChanges();
            }

            var userRole = _context.Roles.IgnoreQueryFilters().FirstOrDefault(r => r.TenantId == _tenantId && r.Name == StaticRoleNames.Tenants.User);
            if (userRole == null)
            {
                userRole = _context.Roles.Add(new Role(_tenantId, StaticRoleNames.Tenants.User, StaticRoleNames.Tenants.User) { IsStatic = true, IsDefault = true }).Entity;
                _context.SaveChanges();
            }
            // Grant all permissions to admin role

            var grantedPermissions = _context.Permissions.IgnoreQueryFilters()
                .OfType<RolePermissionSetting>()
                .Where(p => p.TenantId == _tenantId && p.RoleId == adminRole.Id)
                .Select(p => p.Name)
                .ToList();

            var permissions = PermissionFinder
                .GetAllPermissions(new IFAuthorizationProvider())
                .Where(p => p.MultiTenancySides.HasFlag(MultiTenancySides.Tenant) &&
                            !grantedPermissions.Contains(p.Name))
                .ToList();

            if (permissions.Any())
            {
                _context.Permissions.AddRange(
                    permissions.Select(permission => new RolePermissionSetting
                    {
                        TenantId = _tenantId,
                        Name = permission.Name,
                        IsGranted = true,
                        RoleId = adminRole.Id,
                    })
                );
                _context.SaveChanges();
            }

            //var userGrantedPermissions = _context.Permissions.IgnoreQueryFilters()
            //    .OfType<RolePermissionSetting>()
            //    .Where(p => p.TenantId == _tenantId && p.RoleId == userRole.Id)
            //    .Select(p => p.Name)
            //    .ToList();

            //var userPermissions = PermissionFinder
            //    .GetAllPermissions(new IFAuthorizationProvider())
            //    .Where(p => !new string[] { PermissionNames.Pages_Users, PermissionNames.Pages_Roles }.Contains(p.Name) && p.MultiTenancySides.HasFlag(MultiTenancySides.Tenant) &&
            //                !userGrantedPermissions.Contains(p.Name))
            //    .ToList();

            //if (userPermissions.Any())
            //{
            //    _context.Permissions.AddRange(
            //        userPermissions.Select(permission => new RolePermissionSetting
            //        {
            //            TenantId = _tenantId,
            //            Name = permission.Name,
            //            IsGranted = true,
            //            RoleId = userRole.Id
            //        })
            //    );
            //    _context.SaveChanges();
            //}
            // Admin user

            var adminUser = _context.Users.IgnoreQueryFilters().FirstOrDefault(u => u.TenantId == _tenantId && u.UserName == AbpUserBase.AdminUserName);
            if (adminUser == null)
            {
                adminUser = User.CreateTenantAdminUser(_tenantId, "admin@defaulttenant.com");
                adminUser.Password = new PasswordHasher<User>(new OptionsWrapper<PasswordHasherOptions>(new PasswordHasherOptions())).HashPassword(adminUser, "123qwe");
                adminUser.IsEmailConfirmed = true;
                adminUser.IsActive = true;

                _context.Users.Add(adminUser);
                _context.SaveChanges();

                // Assign Admin role to admin user
                _context.UserRoles.Add(new UserRole(_tenantId, adminUser.Id, adminRole.Id));
                _context.SaveChanges();
            }
        }
    }
}
