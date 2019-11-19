using IF.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IF.Porsche
{
    /// <summary>
    /// 卡劵
    /// </summary>
    [Table("Coupon")]
    public class CouponAggregate : AggregateRootBase<int>
    {
        /// <summary>
        /// 卡劵名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 有效时间
        /// </summary>
        public DateTime ETime { get; set; }

        /// <summary>
        /// 有效性
        /// </summary>
        public string Validity { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [MaxLength(4000)]
        public string Desc { get; set; }

        /// <summary>
        /// 标签
        /// </summary>
        public string Tags { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public float Money { get; set; }

        /// <summary>
        /// 余额
        /// </summary>
        public float Blance { get; set; }

        /// <summary>
        /// 使用数量
        /// </summary>
        public int UseNumber { get; set; }

        /// <summary>
        /// 车型Id
        /// </summary>
        [Column("CModelsId")]
        public int CarId { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; }
    }
}
