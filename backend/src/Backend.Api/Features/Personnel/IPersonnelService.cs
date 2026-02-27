using Backend.Common.Models;

namespace Backend.Features.Personnel;

public interface IPersonnelService
{
    Task<PagedResult<PersonDto>> GetAllAsync(int page, int pageSize, string? search, CancellationToken cancellationToken = default);
    Task<List<PersonDto>> GetAllUnpagedAsync(CancellationToken cancellationToken = default);
    Task<PersonDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PersonDto> CreateAsync(CreatePersonRequest request, CancellationToken cancellationToken = default);
    Task<PersonDto> UpdateAsync(int id, UpdatePersonRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
