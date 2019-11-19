using IF.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IF.Porsche
{
    [Table("porsche_orderuse")]
    public class OrderUseAggregate : AggregateRootBase<int>
    {
        /// <summary>
        /// 订单Id
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// 有效期
        /// </summary>
        public DateTime ETime { get; set; }

        /// <summary>
        /// 消费项目
        /// </summary>
        public string GoodsName { get; set; }

        /// <summary>
        /// 消费金额
        /// </summary>
        public decimal PayAmount { get; set; }

        /// <summary>
        /// 余额
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// 工单号
        /// </summary>
        public string WorkNo { get; set; }

        /// <summary>
        /// 状态 0待核销/1已使用/2已驳回
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(4000)]
        public string Remarks { get; set; }
    }
}
