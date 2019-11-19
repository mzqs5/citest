using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace IF.Common
{
    public class EntityBase<T> : Entity<T>
    {
        public DateTime CreateTime { get; set; } = DateTime.Now;

        public long CreateUserId { get; set; }

        public DateTime? LastModifyTime { get; set; }

        public long? LastModifyUserId { get; set; }

        public DateTime? DeleteTime { get; set; }

        public long? DeleteUserId { get; set; }

        public bool IsDelete { get; set; }
    }
}
