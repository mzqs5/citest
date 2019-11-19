using Abp.Domain.Entities;
using IF.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IF.Porsche
{
    public class GoodsAggregate : AggregateRootBase<int>
    {
        public long ProductId { get; set; }
    }
}
