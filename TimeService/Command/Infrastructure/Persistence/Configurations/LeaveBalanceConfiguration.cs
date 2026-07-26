using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeService.Command.Domain;

namespace TimeService.Command.Infrastructure.Persistence.Configurations;

public sealed class LeaveBalanceConfiguration : IEntityTypeConfiguration<LeaveBalance>
{
    public void Configure(EntityTypeBuilder<LeaveBalance> builder)
    {
        builder.ToTable("leave_balances");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.LeaveType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Available).HasPrecision(10, 2);
        builder.Property(x => x.Used).HasPrecision(10, 2);
        builder.Property(x => x.Pending).HasPrecision(10, 2);
        builder.HasIndex(x => new { x.EmployeeId, x.LeaveType }).IsUnique();
    }
}
