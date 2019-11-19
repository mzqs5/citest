using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IF.Authorization.Users
{
    [Table("porsche_usercmodel")]
    public class UserCModel : Entity<int>
    {
        public long UserId { get; set; }

        /// <summary>
        /// 车型Id
        /// </summary>
        public int CModelId { get; set; }

        /// <summary>
        /// 车架号
        /// </summary>
        public string FrameNo { get; set; }

        /// <summary>
        /// 车型名称
        /// </summary>
        public string CModelName { get; set; }
    }
}
