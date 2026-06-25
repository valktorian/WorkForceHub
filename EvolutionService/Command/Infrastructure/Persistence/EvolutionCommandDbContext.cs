using EvolutionService.Command.Domain;
using Infrastructure.Api.Messaging;
using Microsoft.EntityFrameworkCore;

namespace EvolutionService.Command.Infrastructure.Persistence;

public class EvolutionCommandDbContext : DbContext
{
    public const string Schema = "evolution_command";

    public EvolutionCommandDbContext(DbContextOptions<EvolutionCommandDbContext> options)
        : base(options)
    {
    }

    public DbSet<JobMovement> JobMovements => Set<JobMovement>();
    public DbSet<SalaryChange> SalaryChanges => Set<SalaryChange>();
    public DbSet<Training> Trainings => Set<Training>();
    public DbSet<Reward> Rewards => Set<Reward>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EvolutionCommandDbContext).Assembly);
    }
}
