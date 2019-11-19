using System.ComponentModel.DataAnnotations;
using Abp.Authorization.Users;

namespace IF.Models.TokenAuth
{
    public class AuthenticateModel
    {
        [Required]
        [StringLength(AbpUserBase.MaxEmailAddressLength)]
        public string UserNameOrMobile { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxPlainPasswordLength)]
        public string Password { get; set; }

        public bool RememberClient { get; set; }
    }

    public class MobileAuthenticateModel
    {
        /// <summary>
        /// 手机号码
        /// </summary>
        [Required]
        [StringLength(AbpUserBase.MaxPhoneNumberLength)]
        public string Mobile { get; set; }

        /// <summary>
        /// 短信验证码
        /// </summary>
        [Required]
        [StringLength(6)]
        public string Code { get; set; }

    }

    public class WechatLoginModel
    {
        [Required]
        public string js_code { get; set; }
        [Required]
        public string encryptedData { get; set; }
        [Required]
        public string iv { get; set; }
    }

    public class DecodeInfo
    {
        public string session_key { get; set; }
        public string encryptedData { get; set; }
        public string iv { get; set; }
    }

    public class DecodeUserInfo {
        public string phoneNumber { get; set; }

        public string openId { get; set; }

        public string unionId { get; set; }

        public string nickName { get; set; }

        public string avatarUrl { get; set; }
    }

    public class JsCodeResult
    {
        public string session_key { get; set; }

        public string openid { get; set; }

        public string unionid { get; set; }

        public string errcode { get; set; }

        public string errmsg { get; set; }
    }
}
