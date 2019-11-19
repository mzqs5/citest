using IF.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IF.Porsche
{
    /// <summary>
    /// 预约短信提醒
    /// </summary>
    [Table("porsche_appointmentsms")]
    public class AppointmentSmsAggregate : AggregateRootBase<int>
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 是否已读
        /// </summary>
        public bool IsAlreadyRead { get; set; }

        /// <summary>
        /// 提醒信息
        /// </summary>
        public string Msg { get; set; }
    }
}
