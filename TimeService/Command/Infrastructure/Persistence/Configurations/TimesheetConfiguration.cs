using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeService.Command.Domain;

namespace TimeService.Command.Infrastructure.Persistence.Configurations;

public sealed class TimesheetConfiguration : IEntityTypeConfiguration<Timesheet>
{
    public void Configure(EntityTypeBuilder<Timesheet> builder)
    {
        builder.ToTable("timesheets");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TotalHours).HasPrecision(8, 2);
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ReviewComment).HasMaxLength(2000);
        builder.HasIndex(x => x.AccountId);
        builder.HasIndex(x => x.EmployeeId);
        builder.HasIndex(x => new { x.EmployeeId, x.PeriodStart, x.PeriodEnd }).IsUnique();
    }
}
