namespace Backend.Features.ExpenseTrackers;

public interface IExpenseTrackersRepository
{
    Task<(List<ExpenseTracker> Items, int TotalCount)> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<ExpenseTracker?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ExpenseTracker> CreateAsync(ExpenseTracker entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(ExpenseTracker entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(ExpenseTracker entity, CancellationToken cancellationToken = default);
}
