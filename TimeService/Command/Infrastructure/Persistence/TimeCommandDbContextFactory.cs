using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TimeService.Command.Infrastructure.Persistence;

public sealed class TimeCommandDbContextFactory : IDesignTimeDbContextFactory<TimeCommandDbContext>
{
    public TimeCommandDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=55433;Database=time_write;Username=admin;Password=admin";

        var options = new DbContextOptionsBuilder<TimeCommandDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new TimeCommandDbContext(options);
    }
}
