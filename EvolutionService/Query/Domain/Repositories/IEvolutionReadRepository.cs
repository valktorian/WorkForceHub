namespace EvolutionService.Query.Domain.Repositories;

public interface IEvolutionReadRepository<TModel>
    where TModel : class
{
    Task<TModel?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<long> CountAsync(CancellationToken ct);

    Task<IReadOnlyList<TModel>> GetPagedAsync(int skip, int take, CancellationToken ct);

    Task<IReadOnlyList<TModel>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct);
}
