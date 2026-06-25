using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace EvolutionService.Command.Infrastructure.Persistence;

public class EvolutionCommandDbContextFactory : IDesignTimeDbContextFactory<EvolutionCommandDbContext>
{
    public EvolutionCommandDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=55433;Database=evolution_write;Username=admin;Password=admin";

        var optionsBuilder = new DbContextOptionsBuilder<EvolutionCommandDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new EvolutionCommandDbContext(optionsBuilder.Options);
    }
}
