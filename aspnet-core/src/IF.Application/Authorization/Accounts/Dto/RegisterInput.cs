using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Auditing;
using Abp.Authorization.Users;
using Abp.Extensions;
using IF.Validation;

namespace IF.Authorization.Accounts.Dto
{
    public class RegisterInput
    {
        /// <summary>
        /// 手机号码
        /// </summary>
        [Required]
        [StringLength(AbpUserBase.MaxEmailAddressLength)]
        public string Mobile { get; set; }

        [Required]
        /// <summary>
        /// 短信验证码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [StringLength(AbpUserBase.MaxPlainPasswordLength)]
        [DisableAuditing]
        public string Password { get; set; }

    }
}
