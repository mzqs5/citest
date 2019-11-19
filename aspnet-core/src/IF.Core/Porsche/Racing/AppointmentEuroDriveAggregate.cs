using IF.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IF.Porsche
{
    /// <summary>
    /// 预约追星欧洲
    /// </summary>
    [Table("porsche_appointmenteurodrive")]
    public class AppointmentEuroDriveAggregate : AggregateRootBase<int>
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
        /// 参数
        /// </summary>
        public string Option { get; set; }
    }
}
