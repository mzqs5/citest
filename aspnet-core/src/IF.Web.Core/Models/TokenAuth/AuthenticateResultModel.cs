using System;

namespace IF.Models.TokenAuth
{
    public class AuthenticateResultModel
    {
        /// <summary>
        /// 身份令牌 请在请求头传入（前面加Bearer）
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// 加密访问令牌
        /// </summary>
        public string EncryptedAccessToken { get; set; }

        /// <summary>
        /// 过期秒数
        /// </summary>
        public int ExpireInSeconds { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>
        public long UserId { get; set; }

        public int StoreId { get; set; }

        /// <summary>
        /// 用户手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get { return DateTime.Now; } }

    }
}
