using EvolutionService.Query.Domain;

namespace EvolutionService.Query.Infrastructure.Repositories;

public class TrainingReadRepository : EvolutionReadRepositoryBase<TrainingReadModel>
{
    public TrainingReadRepository(ReadDbContext readDbContext)
        : base(readDbContext.Trainings, x => x.Id, x => x.EmployeeId, x => x.StartDate)
    {
    }
}
