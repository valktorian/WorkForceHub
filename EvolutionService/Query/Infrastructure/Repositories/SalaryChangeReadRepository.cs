using EvolutionService.Query.Domain;

namespace EvolutionService.Query.Infrastructure.Repositories;

public class SalaryChangeReadRepository : EvolutionReadRepositoryBase<SalaryChangeReadModel>
{
    public SalaryChangeReadRepository(ReadDbContext readDbContext)
        : base(readDbContext.SalaryChanges, x => x.Id, x => x.EmployeeId, x => x.EffectiveDate)
    {
    }
}
