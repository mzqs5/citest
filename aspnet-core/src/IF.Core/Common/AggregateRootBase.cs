using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using IF.Authorization.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace IF.Common
{
    public class AggregateRootBase<T> : AggregateRoot<T>, IFullAudited<User>, IAudited<User>, IAudited, ICreationAudited, IHasCreationTime, IModificationAudited, IHasModificationTime, ICreationAudited<User>, IModificationAudited<User>, IFullAudited, IDeletionAudited, IHasDeletionTime, ISoftDelete, IDeletionAudited<User>
    {
        public User LastModifierUser { get; set; }
        public long? CreatorUserId { get; set; }
        public DateTime CreationTime { get; set; }
        public long? LastModifierUserId { get; set; }
        public DateTime? LastModificationTime { get; set; }
        public User DeleterUser { get; set; }
        public long? DeleterUserId { get; set; }
        public DateTime? DeletionTime { get; set; }
        public bool IsDeleted { get; set; }
        public User CreatorUser { get; set; }
    }
}
