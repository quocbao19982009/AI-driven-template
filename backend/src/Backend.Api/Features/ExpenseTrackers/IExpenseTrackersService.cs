using Backend.Common.Models;

namespace Backend.Features.ExpenseTrackers;

public interface IExpenseTrackersService
{
    Task<PagedResult<ExpenseTrackerDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ExpenseTrackerDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ExpenseTrackerDto> CreateAsync(CreateExpenseTrackerRequest request, int userId, CancellationToken cancellationToken = default);
    Task<ExpenseTrackerDto> UpdateAsync(int id, UpdateExpenseTrackerRequest request, int userId, string userRole, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, int userId, string userRole, CancellationToken cancellationToken = default);
}
