 using Abp.Authorization;
using Abp.Localization;
using Abp.MultiTenancy;

namespace IF.Authorization
{
    public class IFAuthorizationProvider : AuthorizationProvider
    {
        public override void SetPermissions(IPermissionDefinitionContext context)
        {
            context.CreatePermission("Pages.Users.View");
            context.CreatePermission("Pages.Roles.View");
            context.CreatePermission("Pages.Members.View");
            context.CreatePermission("Pages.Store.View");
            context.CreatePermission("Pages.Order.View");
            context.CreatePermission("Pages.OrderUse.View");
            context.CreatePermission("Pages.OrderUseLog.View");
            context.CreatePermission("Pages.AppointmentSms.View");
            context.CreatePermission("Pages.Appointment.View");
            context.CreatePermission("Pages.AppointmentTestDrive.View");
            context.CreatePermission("Pages.AppointmentActivity.View");
            context.CreatePermission("Pages.AppointmentWebActivity.View");
            context.CreatePermission("Pages.Activity.View");
            context.CreatePermission("Pages.StoreActivity.View");

            context.CreatePermission("Admin.Users.Edit");
            context.CreatePermission("Admin.Users.Delete");
            context.CreatePermission("Admin.Roles.Edit");
            context.CreatePermission("Admin.Roles.Delete");
            context.CreatePermission("Admin.Members.Edit");
            context.CreatePermission("Admin.Members.Delete");
            context.CreatePermission("Admin.Members.Export");
            context.CreatePermission("Admin.Store.Edit");
            context.CreatePermission("Admin.Store.Delete");
            context.CreatePermission("Admin.Activity.Edit");
            context.CreatePermission("Admin.Activity.Delete");
            context.CreatePermission("Admin.StoreActivity.Edit");
            context.CreatePermission("Admin.StoreActivity.Delete");
            context.CreatePermission("Admin.Appointment.Edit");
            context.CreatePermission("Admin.Appointment.WriteOff");
            context.CreatePermission("Admin.Appointment.Export");
            context.CreatePermission("Admin.AppointmentTestDrive.Edit");
            context.CreatePermission("Admin.AppointmentTestDrive.WriteOff");
            context.CreatePermission("Admin.AppointmentTestDrive.Export");
            context.CreatePermission("Admin.AppointmentActivity.Edit");
            context.CreatePermission("Admin.AppointmentActivity.WriteOff");
            context.CreatePermission("Admin.AppointmentActivity.Export");
            context.CreatePermission("Admin.AppointmentWebActivity.Edit");
            context.CreatePermission("Admin.AppointmentWebActivity.WriteOff");
            context.CreatePermission("Admin.AppointmentWebActivity.Export");
            context.CreatePermission("Admin.Order.Edit");
            context.CreatePermission("Admin.Order.WriteOff");
            context.CreatePermission("Admin.Order.Export");
            context.CreatePermission("Admin.OrderUse.Edit");
            context.CreatePermission("Admin.OrderUse.WriteOff");
            context.CreatePermission("Admin.OrderUse.Export");
            context.CreatePermission("Admin.OrderUseLog.Export");
            context.CreatePermission("Admin.AppointmentSms.Export");
        }

        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, IFConsts.LocalizationSourceName);
        }
    }
}
