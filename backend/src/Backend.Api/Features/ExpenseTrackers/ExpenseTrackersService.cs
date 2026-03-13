using Backend.Common.Exceptions;
using Backend.Common.Models;
using FluentValidation;

namespace Backend.Features.ExpenseTrackers;

public class ExpenseTrackersService : IExpenseTrackersService
{
    private readonly IExpenseTrackersRepository _repository;
    private readonly IValidator<CreateExpenseTrackerRequest> _createValidator;
    private readonly IValidator<UpdateExpenseTrackerRequest> _updateValidator;
    private readonly ILogger<ExpenseTrackersService> _logger;

    public ExpenseTrackersService(
        IExpenseTrackersRepository repository,
        IValidator<CreateExpenseTrackerRequest> createValidator,
        IValidator<UpdateExpenseTrackerRequest> updateValidator,
        ILogger<ExpenseTrackersService> logger)
    {
        _repository = repository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<PagedResult<ExpenseTrackerDto>> GetAllAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        List<string> errors = [];
        if (page < 1)
            errors.Add("Page must be greater than or equal to 1.");
        if (pageSize < 1 || pageSize > 100)
            errors.Add("Page size must be between 1 and 100.");
        if (errors.Count > 0)
            throw new Common.Exceptions.ValidationException(errors);

        var (entities, totalCount) = await _repository.GetAllAsync(page, pageSize, cancellationToken);
        var items = entities.Select(MapToDto).ToList();
        _logger.LogDebug("Retrieved {Count} of {Total} ExpenseTrackers (page {Page})", items.Count, totalCount, page);
        return new PagedResult<ExpenseTrackerDto>(items, totalCount, page, pageSize);
    }

    public async Task<ExpenseTrackerDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("ExpenseTracker {ExpenseTrackerId} not found", id);
            throw new NotFoundException("ExpenseTracker", id);
        }

        return MapToDto(entity);
    }

    public async Task<ExpenseTrackerDto> CreateAsync(
        CreateExpenseTrackerRequest request, int userId, CancellationToken cancellationToken = default)
    {
        await ValidateAndThrowAsync(_createValidator, request, cancellationToken);

        var entity = new ExpenseTracker
        {
            Amount = request.Amount,
            Category = request.Category,
            Description = request.Description,
            Date = request.Date,
            UserId = userId
        };

        var created = await _repository.CreateAsync(entity, cancellationToken);
        _logger.LogInformation("Created ExpenseTracker {ExpenseTrackerId} by user {UserId}", created.Id, userId);
        return MapToDto(created);
    }

    public async Task<ExpenseTrackerDto> UpdateAsync(
        int id, UpdateExpenseTrackerRequest request, int userId, string userRole,
        CancellationToken cancellationToken = default)
    {
        await ValidateAndThrowAsync(_updateValidator, request, cancellationToken);

        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("ExpenseTracker {ExpenseTrackerId} not found for update", id);
            throw new NotFoundException("ExpenseTracker", id);
        }

        EnsureOwnerOrAdmin(entity, userId, userRole);

        entity.Amount = request.Amount;
        entity.Category = request.Category;
        entity.Description = request.Description;
        entity.Date = request.Date;

        await _repository.UpdateAsync(entity, cancellationToken);
        _logger.LogInformation("Updated ExpenseTracker {ExpenseTrackerId} by user {UserId}", entity.Id, userId);
        return MapToDto(entity);
    }

    public async Task DeleteAsync(int id, int userId, string userRole, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("ExpenseTracker {ExpenseTrackerId} not found for deletion", id);
            throw new NotFoundException("ExpenseTracker", id);
        }

        EnsureOwnerOrAdmin(entity, userId, userRole);

        await _repository.DeleteAsync(entity, cancellationToken);
        _logger.LogInformation("Deleted ExpenseTracker {ExpenseTrackerId} by user {UserId}", id, userId);
    }

    private static void EnsureOwnerOrAdmin(ExpenseTracker entity, int userId, string userRole)
    {
        if (entity.UserId != userId && !string.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            throw new ForbiddenAccessException("You can only modify your own expenses.");
        }
    }

    private static ExpenseTrackerDto MapToDto(ExpenseTracker entity) => new(
        entity.Id,
        entity.Amount,
        entity.Category,
        entity.Description,
        entity.Date,
        entity.UserId,
        entity.User is not null ? $"{entity.User.FirstName} {entity.User.LastName}" : "Unknown",
        entity.CreatedAt,
        entity.UpdatedAt
    );

    private static async Task ValidateAndThrowAsync<T>(IValidator<T> validator, T instance, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(instance, ct);
        if (!result.IsValid)
        {
            var errors = result.Errors.Select(e => e.ErrorMessage).ToList();
            throw new Common.Exceptions.ValidationException(errors);
        }
    }
}
