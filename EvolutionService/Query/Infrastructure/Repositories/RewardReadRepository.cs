using EvolutionService.Query.Domain;

namespace EvolutionService.Query.Infrastructure.Repositories;

public class RewardReadRepository : EvolutionReadRepositoryBase<RewardReadModel>
{
    public RewardReadRepository(ReadDbContext readDbContext)
        : base(readDbContext.Rewards, x => x.Id, x => x.EmployeeId, x => x.GrantedAt)
    {
    }
}
