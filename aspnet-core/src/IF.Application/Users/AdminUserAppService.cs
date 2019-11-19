using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Abp;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Zero.Configuration;
using DevExtreme.AspNet.Mvc;
using IF.Authorization.Roles;
using IF.Authorization.Users;
using IF.Configuration;
using IF.Porsche;
using IF.Users.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IF.Authorization
{

    public class AdminUserAppService : IFAppServiceBase, IAdminUserAppService
    {
        IRepository<User, long> AdminUserRepository;
        IRepository<StoreAggregate> StoreRepository;
        IRepository<Role> RoleRepository;
        UserManager userManager;
        public AdminUserAppService(
            IRepository<User, long> AdminUserRepository,
            IRepository<StoreAggregate> StoreRepository,
            IRepository<Role> RoleRepository,
            UserManager userManager)
        {
            this.userManager = userManager;
            this.StoreRepository = StoreRepository;
            this.RoleRepository = RoleRepository;
            this.AdminUserRepository = AdminUserRepository;
        }

        /// <summary>
        /// ��ȡ����Ա�������Ϣ��֧�ַ�ҳ��ѯ
        /// </summary>
        /// <param name="loadOptions">��ѯ����</param>
        /// <returns></returns>
        public async Task<object> GetListDataAsync(DataSourceLoadOptions loadOptions)
        {
            var roles = RoleRepository.GetAll().ToList();
            var result = from AdminUser in AdminUserRepository.GetAll().Include(p => p.Roles).Where(p => !p.Roles.Select(r => r.RoleId).Contains(3)).AsEnumerable()
                         join Store in StoreRepository.GetAll().AsEnumerable()
                         on AdminUser.StoreId equals Store.Id
                         into Stores
                         from Store in Stores.DefaultIfEmpty()
                         select new AdminUserDto
                         {
                             Id = AdminUser.Id,
                             Name = AdminUser.Name,
                             UserName = AdminUser.UserName,
                             StoreId = AdminUser.StoreId,
                             StoreName = Store == null ? "" : Store.Name,
                             RoleNames = roles.Where(p => AdminUser.Roles.Select(r => r.RoleId).ToArray().Contains(p.Id)).Select(p => p.DisplayName).ToArray()
                         };
            return base.DataSourceLoadMap(result, loadOptions);
        }

        #region ����ID��ȡ����Ա��Ϣ
        /// <summary>
        /// ����ID��ȡ����Ա��Ϣ
        /// </summary>
        /// <param name="id">����Ա��Ϣ����</param>
        /// <returns></returns>
        public async Task<AdminUserDto> GetAsync(long id)
        {
            try
            {
                var AdminUser = await AdminUserRepository.GetAll().Where(k => k.Id == id).FirstOrDefaultAsync();
                var roles = await userManager.GetRolesAsync(AdminUser);
                if (AdminUser == null)
                {
                    throw new EntityNotFoundException(typeof(User), id);
                }
                var dto = this.ObjectMapper.Map<AdminUserDto>(AdminUser);
                dto.RoleNames = roles.ToArray();
                return dto;
            }
            catch (Exception e)
            {
                Logger.Error("����Ա��ȡ�쳣��", e);
                throw new AbpException("����Ա��ȡ�쳣", e);
            }
        }
        #endregion

        #region  �������߸��Ĺ���Ա��Ϣ
        /// <summary>
        /// �������߸��Ĺ���Ա��Ϣ
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        [AbpAuthorize("Admin.Users.Edit")]
        public async Task SaveAsync(AdminUserEditDto input)
        {
            User entity = this.ObjectMapper.Map<User>(input);
            if (input.Id != 0)
            {
                User AdminUserAggregate = AdminUserRepository.GetAll().Include(r => r.Roles).Where(k => k.Id == input.Id).FirstOrDefault();
                AdminUserAggregate.Name = entity.Name;
                AdminUserAggregate.UserName = entity.UserName;
                AdminUserAggregate.EmailAddress = entity.UserName;
                AdminUserAggregate.StoreId = entity.StoreId;

                await AdminUserRepository.UpdateAsync(AdminUserAggregate);
                var result = await userManager.SetRoles(AdminUserAggregate, input.RoleNames);
                if (!result.Succeeded)
                    throw new AbpException(string.Join(";", result.Errors.Select(e => e.Description)));
            }
            else
            {
                entity.TenantId = AbpSession.TenantId;
                entity.IsEmailConfirmed = true;
                entity.EmailAddress = entity.UserName;
                await userManager.InitializeOptionsAsync(AbpSession.TenantId);
                var result = await userManager.CreateAsync(entity, input.Password);
                if (!result.Succeeded)
                    throw new AbpException(string.Join(";", result.Errors.Select(e => e.Description)));
                if (input.RoleNames != null)
                {
                    result = await userManager.SetRoles(entity, input.RoleNames);
                    if (!result.Succeeded)
                        throw new AbpException(string.Join(";", result.Errors.Select(e => e.Description)));
                }
            }
            await CurrentUnitOfWork.SaveChangesAsync();
        }
        #endregion

        #region ��������Ա��Ĭ�ϲ���
        /// <summary>
        /// ��������Ա��Ĭ�ϲ���
        /// </summary>
        /// <returns></returns>
        public async Task<AdminUserEditDto> GetEmptyEntityAsync()
        {
            return await Task.Run(() => new AdminUserEditDto());
        }
        #endregion

        #region ����ɾ������Ա
        /// <summary>
        /// ����ɾ������Ա
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete]
        [AbpAuthorize("Admin.Users.Delete")]
        public async Task BatchDeleteAsync(string ids)
        {
            try
            {
                foreach (var id in ids.Split(','))
                {
                    await AdminUserRepository.DeleteAsync(kv => kv.Id == int.Parse(id));
                }
                await CurrentUnitOfWork.SaveChangesAsync();

            }
            catch (Exception e)
            {
                Logger.Error("����Աɾ���쳣��", e);
                throw new AbpException("����Աɾ���쳣��", e);
            }
        }
        #endregion

    }

    public class AdminUserEditDto
    {
        public long Id { get; set; }

        public string UserName { get; set; }

        public string Name { get; set; }

        public int StoreId { get; set; }

        public string Password { get; set; }

        public string[] RoleNames { get; set; }
    }

    public class AdminUserDto
    {
        public long Id { get; set; }

        public string UserName { get; set; }

        public string Name { get; set; }

        public int StoreId { get; set; }

        public string StoreName { get; set; }

        public string[] RoleNames { get; set; }
    }
}

