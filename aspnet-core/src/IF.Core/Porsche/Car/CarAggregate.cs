
using Abp.Domain.Entities;
using IF.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
namespace IF.Porsche
{
    [Table("Cars")]
    public class CarAggregate : AggregateRootBase<int>
    {
        /// <summary>
        /// 车型名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 类型 1保时捷，2阿斯顿马丁
        /// </summary>
        public int Type { get; set; }
    }
}
