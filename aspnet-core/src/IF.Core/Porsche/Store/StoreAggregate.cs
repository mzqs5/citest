using Abp.Domain.Entities;
using IF.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IF.Porsche
{
    [Table("Store")]
    public class StoreAggregate : AggregateRootBase<int>
    {
        /// <summary>
        /// 经销商名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 经销商名称 英文
        /// </summary>
        public string EName { get; set; }

        /// <summary>
        /// 经销商电话
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 经销商电话英文
        /// </summary>
        public string EPhone { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 地址英文
        /// </summary>
        public string EAddress { get; set; }

        /// <summary>
        /// 营业时间
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// 营业时间英文
        /// </summary>
        public string EDate { get; set; }

        /// <summary>
        /// 更多
        /// </summary>
        public string More { get; set; }


        /// <summary>
        /// 更多英文
        /// </summary>
        public string EMore { get; set; }

        /// <summary>
        /// 类型 1保时捷，2阿斯顿马丁
        /// </summary>
        public int Type { get; set; }
    }
}
