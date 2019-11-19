using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using IF.Configuration;
using IF.Web;

namespace IF.EntityFrameworkCore
{
    /* This class is needed to run "dotnet ef ..." commands from command line on development. Not used anywhere else */
    public class IFDbContextFactory : IDesignTimeDbContextFactory<IFDbContext>
    {
        public IFDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<IFDbContext>();
            var configuration = AppConfigurations.Get(WebContentDirectoryFinder.CalculateContentRootFolder());

            IFDbContextConfigurer.Configure(builder, configuration.GetConnectionString(IFConsts.ConnectionStringName));

            return new IFDbContext(builder.Options);
        }
    }
}
