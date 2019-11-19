using System;
using Abp;
using Abp.Authorization;
using Abp.Dependency;
using Abp.UI;

namespace IF.Authorization
{
    public class AbpLoginResultTypeHelper : AbpServiceBase, ITransientDependency
    {
        public AbpLoginResultTypeHelper()
        {
            LocalizationSourceName = IFConsts.LocalizationSourceName;
        }

        public AbpException CreateExceptionForFailedLoginAttempt(AbpLoginResultType result, string usernameOrEmailAddress, string tenancyName)
        {
            switch (result)
            {
                case AbpLoginResultType.Success:
                    return new AbpException("Don't call this method with a success result!");
                case AbpLoginResultType.InvalidUserNameOrEmailAddress:
                case AbpLoginResultType.InvalidPassword:
                    return new UserFriendlyException("LoginFailed：InvalidUserNameOrPassword", "InvalidUserNameOrPassword");
                case AbpLoginResultType.InvalidTenancyName:
                    return new UserFriendlyException("LoginFailed：ThereIsNoTenantDefinedWithName", "ThereIsNoTenantDefinedWithName");
                case AbpLoginResultType.TenantIsNotActive:
                    return new UserFriendlyException("LoginFailed：TenantIsNotActive", "TenantIsNotActive");
                case AbpLoginResultType.UserIsNotActive:
                    return new UserFriendlyException("LoginFailed：UserIsNotActiveAndCanNotLogin", "UserIsNotActiveAndCanNotLogin");
                case AbpLoginResultType.UserEmailIsNotConfirmed:
                    return new UserFriendlyException("LoginFailed：UserEmailIsNotConfirmedAndCanNotLogin", "UserEmailIsNotConfirmedAndCanNotLogin");
                case AbpLoginResultType.LockedOut:
                    return new UserFriendlyException("LoginFailed：UserLockedOutMessage", "UserLockedOutMessage");
                default: // Can not fall to default actually. But other result types can be added in the future and we may forget to handle it
                    Logger.Warn("Unhandled login fail reason: " + result);
                    return new UserFriendlyException("LoginFailed");
            }
        }

        public string CreateLocalizedMessageForFailedLoginAttempt(AbpLoginResultType result, string usernameOrEmailAddress, string tenancyName)
        {
            switch (result)
            {
                case AbpLoginResultType.Success:
                    throw new Exception("Don't call this method with a success result!");
                case AbpLoginResultType.InvalidUserNameOrEmailAddress:
                case AbpLoginResultType.InvalidPassword:
                    return "InvalidUserNameOrPassword";
                case AbpLoginResultType.InvalidTenancyName:
                    return "ThereIsNoTenantDefinedWithName";
                case AbpLoginResultType.TenantIsNotActive:
                    return "TenantIsNotActive";
                case AbpLoginResultType.UserIsNotActive:
                    return "UserIsNotActiveAndCanNotLogin";
                case AbpLoginResultType.UserEmailIsNotConfirmed:
                    return "UserEmailIsNotConfirmedAndCanNotLogin";
                default: // Can not fall to default actually. But other result types can be added in the future and we may forget to handle it
                    Logger.Warn("Unhandled login fail reason: " + result);
                    return "LoginFailed";
            }
        }
    }
}
