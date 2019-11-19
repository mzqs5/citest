//using Microsoft.EntityFrameworkCore;
//using Abp.Zero.EntityFrameworkCore;
//using IF.Authorization.Roles;
//using IF.Authorization.Users;
//using IF.MultiTenancy;
//using IF.Porsche;
//using System.Threading.Tasks;
//using JetBrains.Annotations;
//using Microsoft.EntityFrameworkCore.ChangeTracking;
//using Abp.EntityFrameworkCore;

//namespace IF.EntityFrameworkCore
//{
//    public class OutSideDbContext : AbpDbContext
//    {
//        public virtual DbSet<GoodsAggregate> Goods { get; set; }

//        public virtual DbSet<CouponAggregate> Coupons { get; set; }

//        public OutSideDbContext(DbContextOptions<OutSideDbContext> options)
//            : base(options)
//        {
//        }

//        protected override void OnModelCreating(ModelBuilder modelBuilder)
//        {
//            base.OnModelCreating(modelBuilder);

//        }

//    }
//}
