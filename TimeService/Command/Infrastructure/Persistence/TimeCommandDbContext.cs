using Infrastructure.Api.Messaging;
using Microsoft.EntityFrameworkCore;
using TimeService.Command.Domain;

namespace TimeService.Command.Infrastructure.Persistence;

public class TimeCommandDbContext : DbContext
{
    public const string Schema = "time_command";

    public TimeCommandDbContext(DbContextOptions<TimeCommandDbContext> options)
        : base(options)
    {
    }

    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
    public DbSet<Timesheet> Timesheets => Set<Timesheet>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TimeCommandDbContext).Assembly);
    }
}
