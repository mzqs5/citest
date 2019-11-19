
using Abp.Domain.Entities;
using IF.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
namespace IF.Porsche
{
    [Table("CModels")]
    public class CModelAggregate : AggregateRootBase<int>
    {
        /// <summary>
        /// 车型名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 大图
        /// </summary>
        public string BgPic { get; set; }

        /// <summary>
        /// 小图
        /// </summary>
        public string Pic { get; set; }
    }
}
