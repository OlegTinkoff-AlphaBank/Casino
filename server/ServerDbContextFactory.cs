using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using serverDB;
namespace server
{
    public class ServerDbContextFactory : IDesignTimeDbContextFactory<ServerDbContext>
    {
        public ServerDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddUserSecrets<Program>()
                .Build();

            var connectionString = config.GetConnectionString("Default");

            var optionsBuilder = new DbContextOptionsBuilder<ServerDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new ServerDbContext(optionsBuilder.Options);
        }
    }
}