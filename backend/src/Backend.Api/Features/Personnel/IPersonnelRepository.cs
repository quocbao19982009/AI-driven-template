namespace Backend.Features.Personnel;

public interface IPersonnelRepository
{
    Task<(List<Person> Items, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken = default);

    Task<List<Person>> GetAllUnpagedAsync(CancellationToken cancellationToken = default);
    Task<Person?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithPersonalIdAsync(string personalId, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsWithEmailAsync(string email, int? excludeId = null, CancellationToken cancellationToken = default);
    Task<Person> CreateAsync(Person entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Person entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Person entity, CancellationToken cancellationToken = default);
}
