using EvolutionService.Query.Domain.Repositories;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace EvolutionService.Query.Infrastructure.Repositories;

public class EvolutionReadRepositoryBase<TModel> : IEvolutionReadRepository<TModel>
    where TModel : class
{
    private readonly IMongoCollection<TModel> _collection;
    private readonly Expression<Func<TModel, Guid>> _idSelector;
    private readonly Expression<Func<TModel, Guid>> _employeeSelector;
    private readonly Expression<Func<TModel, object>> _sortSelector;

    public EvolutionReadRepositoryBase(
        IMongoCollection<TModel> collection,
        Expression<Func<TModel, Guid>> idSelector,
        Expression<Func<TModel, Guid>> employeeSelector,
        Expression<Func<TModel, object>> sortSelector)
    {
        _collection = collection;
        _idSelector = idSelector;
        _employeeSelector = employeeSelector;
        _sortSelector = sortSelector;
    }

    public Task<TModel?> GetByIdAsync(Guid id, CancellationToken ct)
        => _collection.Find(Builders<TModel>.Filter.Eq(_idSelector, id)).FirstOrDefaultAsync(ct)!;

    public Task<long> CountAsync(CancellationToken ct)
        => _collection.CountDocumentsAsync(FilterDefinition<TModel>.Empty, cancellationToken: ct);

    public async Task<IReadOnlyList<TModel>> GetPagedAsync(int skip, int take, CancellationToken ct)
        => await _collection.Find(_ => true)
            .Sort(Builders<TModel>.Sort.Descending(_sortSelector))
            .Skip(skip)
            .Limit(take)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<TModel>> GetByEmployeeAsync(Guid employeeId, CancellationToken ct)
        => await _collection.Find(Builders<TModel>.Filter.Eq(_employeeSelector, employeeId))
            .Sort(Builders<TModel>.Sort.Descending(_sortSelector))
            .ToListAsync(ct);
}
