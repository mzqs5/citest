using System.Data.Common;
using Abp.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IF.EntityFrameworkCore
{
    public static class IFDbContextConfigurer
    {
        //public static void Configure(DbContextOptionsBuilder<IFDbContext> builder, string connectionString)
        //{
        //    builder.UseSqlServer(connectionString);
        //}

        //public static void Configure(DbContextOptionsBuilder<IFDbContext> builder, DbConnection connection)
        //{
        //    builder.UseSqlServer(connection);
        //}

        public static void Configure<T>(
            DbContextOptionsBuilder<T> dbContextOptions,
            string connectionString
            )
            where T : AbpDbContext
        {
            /* This is the single point to configure DbContextOptions for TaobaoAuthorizationDbContext */
            //dbContextOptions.UseSqlServer(connectionString);
            dbContextOptions.UseSqlServer(connectionString);
        }

        public static void Configure<T>(
            DbContextOptionsBuilder<T> dbContextOptions,
            DbConnection connection
            )
            where T : AbpDbContext
        {
            /* This is the single point to configure DbContextOptions for TaobaoAuthorizationDbContext */
            //dbContextOptions.UseSqlServer(connectionString);
            dbContextOptions.UseSqlServer(connection);
        }

    }
}
