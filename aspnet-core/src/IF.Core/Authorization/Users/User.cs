using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Authorization.Users;
using Abp.Extensions;

namespace IF.Authorization.Users
{
    public class User : AbpUser<User>
    {
        public new string Name { get; set; }

        public new string Surname { get; set; }

        public string Mobile { get; set; }

        public string UnionId { get; set; }

        /// <summary>
        /// 最后一次门店Id
        /// </summary>
        public int LastStoreId { get; set; }

        public int Sex { get; set; }

        public int IsCarOwner { get; set; }

        public int StoreId { get; set; }

        [ForeignKey("UserId")]
        public List<Wechat> Wechats { get; set; }

        [ForeignKey("UserId")]
        public List<UserCar> UserCars { get; set; }

        [ForeignKey("UserId")]
        public List<UserCModel> UserCModels { get; set; }


        [ForeignKey("UserId")]
        public List<Contacts> Contacts { get; set; }

        [ForeignKey("UserId")]
        public List<Address> Address { get; set; }

        public const string DefaultPassword = "123qwe";

        public User() : base()
        {
            Wechats = new List<Wechat>();
            UserCars = new List<UserCar>();
            UserCModels = new List<UserCModel>();
            Contacts = new List<Contacts>();
            Address = new List<Address>();
        }

        public static string CreateRandomPassword()
        {
            return Guid.NewGuid().ToString("N").Truncate(16);
        }

        public static User CreateTenantAdminUser(int tenantId, string emailAddress)
        {
            var user = new User
            {
                TenantId = tenantId,
                UserName = AdminUserName,
                Name = AdminUserName,
                Surname = AdminUserName,
                EmailAddress = emailAddress,
                Roles = new List<UserRole>()
            };

            user.SetNormalizedNames();

            return user;
        }
    }
}
