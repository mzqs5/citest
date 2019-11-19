using Abp.Domain.Entities;
using IF.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IF.Porsche
{
    [Table("Dealer")]
    public class DealerAggregate : AggregateRootBase<int>
    {
        /// <summary>
        /// 
        /// </summary>
        public string AUserId { get; set; }
        /// <summary>
        /// 经销商名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 经销商详情
        /// </summary>
        public string DDesc { get; set; }

        /// <summary>
        /// 经销商电话
        /// </summary>
        public string Phone { get; set; }

        public int System_PlaceId { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 经度
        /// </summary>
        public string Longitude { get; set; }

        /// <summary>
        /// 纬度
        /// </summary>
        public string Latitude { get; set; }

        /// <summary>
        /// 折扣
        /// </summary>
        public float? Distance { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; }
    }
}
