using IF.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IF.Porsche
{
    /// <summary>
    /// 预约赛事
    /// </summary>
    [Table("porsche_appointmentracing")]
    public class AppointmentRacingAggregate : AggregateRootBase<int>
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public long UserId { get; set; }

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
        /// 门票类型
        /// </summary>
        public string Option { get; set; }

        /// <summary>
        /// 赛事Id
        /// </summary>
        public int RacingId { get; set; }
    }
}
