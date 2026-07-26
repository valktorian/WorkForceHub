using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeService.Command.Domain;

namespace TimeService.Command.Infrastructure.Persistence.Configurations;

public sealed class TimeEntryConfiguration : IEntityTypeConfiguration<TimeEntry>
{
    public void Configure(EntityTypeBuilder<TimeEntry> builder)
    {
        builder.ToTable("time_entries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ProjectCode).HasMaxLength(100).IsRequired();
        builder.Property(x => x.TaskCode).HasMaxLength(100).IsRequired();
        builder.Property(x => x.StartTime).HasMaxLength(5).IsRequired();
        builder.Property(x => x.EndTime).HasMaxLength(5).IsRequired();
        builder.Property(x => x.Hours).HasPrecision(8, 2);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.HasIndex(x => x.AccountId);
        builder.HasIndex(x => x.EmployeeId);
        builder.HasIndex(x => x.WorkDate);
    }
}
