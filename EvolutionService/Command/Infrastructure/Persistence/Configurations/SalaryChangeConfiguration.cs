using EvolutionService.Command.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EvolutionService.Command.Infrastructure.Persistence.Configurations;

public class SalaryChangeConfiguration : IEntityTypeConfiguration<SalaryChange>
{
    public void Configure(EntityTypeBuilder<SalaryChange> builder)
    {
        builder.ToTable("salary_changes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Currency).HasMaxLength(12).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Comment).HasMaxLength(1000);
        builder.Property(x => x.PreviousSalary).HasPrecision(18, 2);
        builder.Property(x => x.NewSalary).HasPrecision(18, 2);
        builder.HasIndex(x => x.EmployeeId);
        builder.HasIndex(x => x.EffectiveDate);
    }
}
