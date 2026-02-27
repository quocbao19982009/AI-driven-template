using Backend.Common.Models;

namespace Backend.Features.Factories;

public interface IFactoriesService
{
    Task<PagedResult<FactoryDto>> GetAllAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default);
    Task<List<FactoryDto>> GetAllUnpagedAsync(CancellationToken cancellationToken = default);
    Task<FactoryDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<FactoryDto> CreateAsync(CreateFactoryRequest request, CancellationToken cancellationToken = default);
    Task<FactoryDto> UpdateAsync(int id, UpdateFactoryRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
