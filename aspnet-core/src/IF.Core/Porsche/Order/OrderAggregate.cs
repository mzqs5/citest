using Abp.Domain.Entities;
using IF.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IF.Porsche
{
    [Table("porsche_order")]
    public class OrderAggregate : AggregateRootBase<int>
    {
        /// <summary>
        /// 会员Id
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// 经销商Id
        /// </summary>
        public int DealerId { get; set; }
        /// <summary>
        /// 订单编号
        /// </summary>
        public string OrderNo { get; set; }

        /// <summary>
        /// 总金额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 余额
        /// </summary>
        public decimal Balance { get; set; }

        /// <summary>
        /// 总数量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 状态 0待付款/1已付款
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// 状态 0未使用/1使用中/2已使用/3失效
        /// </summary>
        public int Progress { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(4000)]
        public string Remarks { get; set; }

        /// <summary>
        /// 支付方式
        /// </summary>
        public int PayMode { get; set; }

        /// <summary>
        /// 支付凭证
        /// </summary>
        public string PayCer { get; set; }

        /// <summary>
        /// 支付时间
        /// </summary>
        public DateTime PayTime { get; set; }

        /// <summary>
        /// 订单详细
        /// </summary>
        [ForeignKey("OrderId")]
        public List<OrderItem> OrderItems { get; set; }
    }
}
