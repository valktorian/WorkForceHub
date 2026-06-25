using EvolutionService.Query.Domain;
using EvolutionService.Query.Domain.Repositories;

namespace EvolutionService.Query.Infrastructure.Repositories;

public class JobMovementReadRepository : EvolutionReadRepositoryBase<JobMovementReadModel>
{
    public JobMovementReadRepository(ReadDbContext readDbContext)
        : base(readDbContext.JobMovements, x => x.Id, x => x.EmployeeId, x => x.EffectiveDate)
    {
    }
}
