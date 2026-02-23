namespace Backend.Features._FeatureTemplate;

// TODO: Rename to match your entity (e.g., IProductsRepository)

public interface IFeatureRepository
{
    Task<(List<Feature> Items, int TotalCount)> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<Feature?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Feature> CreateAsync(Feature entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Feature entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Feature entity, CancellationToken cancellationToken = default);
}
