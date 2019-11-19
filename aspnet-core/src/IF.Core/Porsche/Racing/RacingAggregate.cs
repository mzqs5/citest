using IF.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IF.Porsche
{
    /// <summary>
    /// 赛事
    /// </summary>
    [Table("porsche_racing")]
    public class RacingAggregate : AggregateRootBase<int>
    {
        /// <summary>
        /// 轮播图
        /// </summary>
        public string Imgs { get; set; }

        /// <summary>
        /// 轮播图移动端
        /// </summary>
        public string MobileImgs { get; set; }

    }
}
