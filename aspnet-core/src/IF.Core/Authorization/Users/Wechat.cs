using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IF.Authorization.Users
{
    [Table("porsche_wechat")]
    public class Wechat : Entity<int>
    {
        public long UserId { get; set; }

        public string OpenId { get; set; }

        public string NickName { get; set; }

        public string Avater { get; set; }

        /// <summary>
        /// 0 Web 1公众号 2小程序
        /// </summary>
        public int Type { get; set; }
    }
}
