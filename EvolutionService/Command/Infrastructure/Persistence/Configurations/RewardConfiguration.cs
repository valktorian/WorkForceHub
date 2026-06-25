using EvolutionService.Command.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EvolutionService.Command.Infrastructure.Persistence.Configurations;

public class RewardConfiguration : IEntityTypeConfiguration<Reward>
{
    public void Configure(EntityTypeBuilder<Reward> builder)
    {
        builder.ToTable("rewards");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Type).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Comment).HasMaxLength(1000);
        builder.Property(x => x.Value).HasPrecision(18, 2);
        builder.HasIndex(x => x.EmployeeId);
        builder.HasIndex(x => x.GrantedAt);
    }
}
