using IF.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IF.Porsche
{
    /// <summary>
    /// 预约试驾
    /// </summary>
    [Table("porsche_appointmenttestdrive")]
    public class AppointmentTestDriveAggregate : AggregateRootBase<int>
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 性别 0男 1女
        /// </summary>
        public int Sex { get; set; }

        /// <summary>
        /// 门店Id
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// 预约车型
        /// </summary>
        public int CarId { get; set; }

        /// <summary>
        /// 姓
        /// </summary>
        public string SurName { get; set; }

        /// <summary>
        /// 名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        public string Mobile { get; set; }

        /// <summary>
        /// 预约日期
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// 状态 0未到店/1已到店
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 联系状态 0未联系/1联系失败/2延迟提醒/3确认预约
        /// </summary>
        public int ContactState { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(4000)]
        public string Remarks { get; set; }
    }
}
