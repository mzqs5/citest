using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IF.Authorization.Users
{
    [Table("porsche_address")]
    public class Address : Entity<int>
    {
        public long UserId { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// 地区
        /// </summary>
        public string Area { get; set; }

        /// <summary>
        /// 街道
        /// </summary>
        public string Street { get; set; }

        /// <summary>
        /// 详细地址
        /// </summary>
        public string DetailAddress { get; set; }

        /// <summary>
        /// 是否默认地址
        /// </summary>
        public Boolean IsDefault { get; set; }
    }
}
