namespace Backend.Features.Factories;

public interface IFactoriesRepository
{
    Task<(List<Factory> Items, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default);

    Task<List<Factory>> GetAllUnpagedAsync(CancellationToken cancellationToken = default);
    Task<Factory?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithNameAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<Factory> CreateAsync(Factory entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Factory entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Factory entity, CancellationToken cancellationToken = default);
}
