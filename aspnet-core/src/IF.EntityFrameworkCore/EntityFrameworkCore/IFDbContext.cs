using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using IF.Authorization.Roles;
using IF.Authorization.Users;
using IF.MultiTenancy;
using IF.Porsche;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace IF.EntityFrameworkCore
{
    public class IFDbContext : AbpZeroDbContext<Tenant, Role, User, IFDbContext>
    {
        /* Define a DbSet for each entity of the application */
        public virtual DbSet<CarAggregate> Cars { get; set; }

        //public virtual DbSet<GoodsAggregate> Goods { get; set; }

        public virtual DbSet<CModelAggregate> CModels { get; set; }
        public virtual DbSet<StoreAggregate> Stores { get; set; }

        public virtual DbSet<DealerAggregate> Dealers { get; set; }

        public virtual DbSet<OrderAggregate> Orders { get; set; }

        public virtual DbSet<OrderUseAggregate> OrderUses { get; set; }

        public virtual DbSet<ActivityAggregate> Activitys { get; set; }

        public virtual DbSet<StoreActivityAggregate> StoreActivitys { get; set; }

        public virtual DbSet<AppointmentAggregate> Appointments { get; set; }

        public virtual DbSet<AppointmentActivityAggregate> AppointmentActivitys { get; set; }
        public virtual DbSet<AppointmentTestDriveAggregate> AppointmentTestDrives { get; set; }
        public virtual DbSet<AppointmentRacingAggregate> AppointmentRacings { get; set; }
        public virtual DbSet<AppointmentWebActivityAggregate> WebActivitys { get; set; }
        public virtual DbSet<SmsAggregate> Smss { get; set; }
        public virtual DbSet<RacingAggregate> Racings { get; set; }
        public virtual DbSet<ShoppingCartAggregate> ShoppingCarts { get; set; }

        public virtual DbSet<GoodsAggregate> Goods { get; set; }
        public virtual DbSet<StoreWxPayAggregate> StoreWxPays { get; set; }
        public virtual DbSet<AppointmentEuroDriveAggregate> AppointmentEuroDrives { get; set; }

        public virtual DbSet<AppointmentSmsAggregate> AppointmentSms { get; set; }
        public virtual DbSet<FileAggregate> Files { get; set; }
        public IFDbContext(DbContextOptions<IFDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.Entity<User>().Ignore(a => a.Name);
            //modelBuilder.Entity<User>().Ignore(a => a.Surname);
            //modelBuilder.Entity<User>().Property(a => a.EmailAddress).IsRequired(false);
        }

        #region 


        #endregion
    }
}
