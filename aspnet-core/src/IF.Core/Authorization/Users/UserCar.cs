using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IF.Authorization.Users
{
    [Table("porsche_usercar")]
    public class UserCar : Entity<int>
    {
        public long UserId { get; set; }

        /// <summary>
        /// 车型Id
        /// </summary>
        public int CarId { get; set; }

        /// <summary>
        /// 车架号
        /// </summary>
        public string FrameNo { get; set; }

        /// <summary>
        /// 车型名称
        /// </summary>
        public string CarName { get; set; }

        /// <summary>
        /// 类型 1保时捷，2阿斯顿马丁
        /// </summary>
        public string Type { get; set; }
    }
}
