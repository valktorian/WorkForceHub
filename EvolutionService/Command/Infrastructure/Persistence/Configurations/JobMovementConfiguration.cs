using EvolutionService.Command.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EvolutionService.Command.Infrastructure.Persistence.Configurations;

public class JobMovementConfiguration : IEntityTypeConfiguration<JobMovement>
{
    public void Configure(EntityTypeBuilder<JobMovement> builder)
    {
        builder.ToTable("job_movements");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PreviousJobTitle).HasMaxLength(120).IsRequired();
        builder.Property(x => x.NewJobTitle).HasMaxLength(120).IsRequired();
        builder.Property(x => x.PreviousDepartment).HasMaxLength(120).IsRequired();
        builder.Property(x => x.NewDepartment).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Comment).HasMaxLength(1000);
        builder.HasIndex(x => x.EmployeeId);
        builder.HasIndex(x => x.EffectiveDate);
    }
}
