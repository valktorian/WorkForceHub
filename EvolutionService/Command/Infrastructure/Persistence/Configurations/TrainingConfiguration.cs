using EvolutionService.Command.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EvolutionService.Command.Infrastructure.Persistence.Configurations;

public class TrainingConfiguration : IEntityTypeConfiguration<Training>
{
    public void Configure(EntityTypeBuilder<Training> builder)
    {
        builder.ToTable("trainings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Provider).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(80).IsRequired();
        builder.Property(x => x.CertificateUrl).HasMaxLength(2000);
        builder.Property(x => x.Comment).HasMaxLength(1000);
        builder.HasIndex(x => x.EmployeeId);
        builder.HasIndex(x => x.StartDate);
        builder.HasIndex(x => x.Status);
    }
}
