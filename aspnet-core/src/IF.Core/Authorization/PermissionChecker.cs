using Abp.Authorization;
using IF.Authorization.Roles;
using IF.Authorization.Users;

namespace IF.Authorization
{
    public class PermissionChecker : PermissionChecker<Role, User>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {
        }
    }
}
