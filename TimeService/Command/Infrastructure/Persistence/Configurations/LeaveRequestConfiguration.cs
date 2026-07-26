using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeService.Command.Domain;

namespace TimeService.Command.Infrastructure.Persistence.Configurations;

public sealed class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.ToTable("leave_requests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.LeaveType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(2000);
        builder.Property(x => x.DecisionComment).HasMaxLength(2000);
        builder.HasIndex(x => x.AccountId);
        builder.HasIndex(x => x.EmployeeId);
        builder.HasIndex(x => x.Status);
    }
}
