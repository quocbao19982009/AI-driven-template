using Backend.Common.Models;

namespace Backend.Features._FeatureTemplate;

// TODO: Rename to match your entity (e.g., IProductsService)

public interface IFeatureService
{
    Task<PagedResult<FeatureDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<FeatureDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<FeatureDto> CreateAsync(CreateFeatureRequest request, CancellationToken cancellationToken = default);
    Task<FeatureDto> UpdateAsync(int id, UpdateFeatureRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
