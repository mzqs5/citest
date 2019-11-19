using Abp.Domain.Entities;
using IF.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IF.Porsche
{
    [Table("porsche_orderitem")]
    public class OrderItem : EntityBase<int>
    {
        public int OrderId { get; set; }

        /// <summary>
        /// 产品Id
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 产品名称
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// 车型Id
        /// </summary>
        public string CModelIds { get; set; }

        /// <summary>
        /// 车型
        /// </summary>
        public string CModels { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 单价
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 总额
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 产品图片
        /// </summary>
        public string ProductPic { get; set; }

        /// <summary>
        /// 卡劵有效期
        /// </summary>
        public DateTime CouponETime { get; set; }

        /// <summary>
        /// 有效期
        /// </summary>
        public string Validity { get; set; }
    }
}
