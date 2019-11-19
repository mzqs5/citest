using Abp.Domain.Entities;
using IF.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IF.Porsche
{
    /// <summary>
    /// 门店活动
    /// </summary>
    [Table("porsche_storeactivity")]
    public class StoreActivityAggregate : AggregateRootBase<int>
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 所属门店Id
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [StringLength(500)]
        public string Desc { get; set; }

        /// <summary>
        /// 封面图
        /// </summary>
        public string ImgUrl { get; set; }


        /// <summary>
        /// 正文
        /// </summary>
        [StringLength(40000)]
        public string Detail { get; set; }
    }

}
