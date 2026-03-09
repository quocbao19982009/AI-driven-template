using Backend.Common.Exceptions;
using Backend.Common.Models;
using FluentValidation;

namespace Backend.Features.Todos;

public class TodosService : ITodosService
{
    private readonly ITodosRepository _repository;
    private readonly IValidator<CreateTodoRequest> _createValidator;
    private readonly IValidator<UpdateTodoRequest> _updateValidator;
    private readonly ILogger<TodosService> _logger;

    public TodosService(
        ITodosRepository repository,
        IValidator<CreateTodoRequest> createValidator,
        IValidator<UpdateTodoRequest> updateValidator,
        ILogger<TodosService> logger)
    {
        _repository = repository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<PagedResult<TodoDto>> GetAllAsync(
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
        _logger.LogDebug("Retrieved {Count} of {Total} Todos (page {Page})", items.Count, totalCount, page);
        return new PagedResult<TodoDto>(items, totalCount, page, pageSize);
    }

    public async Task<TodoDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Todo {TodoId} not found", id);
            throw new NotFoundException("Todo", id);
        }

        return MapToDto(entity);
    }

    public async Task<TodoDto> CreateAsync(CreateTodoRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAndThrowAsync(_createValidator, request, cancellationToken);

        var entity = new Todo
        {
            Title = request.Title,
            Description = request.Description,
            IsCompleted = false,
            DueDate = request.DueDate
        };

        var created = await _repository.CreateAsync(entity, cancellationToken);
        _logger.LogInformation("Created Todo {TodoId} with title {TodoTitle}", created.Id, created.Title);
        return MapToDto(created);
    }

    public async Task<TodoDto> UpdateAsync(int id, UpdateTodoRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAndThrowAsync(_updateValidator, request, cancellationToken);

        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Todo {TodoId} not found for update", id);
            throw new NotFoundException("Todo", id);
        }

        entity.Title = request.Title;
        entity.Description = request.Description;
        entity.DueDate = request.DueDate;
        // IsCompleted is intentionally not updated here — use ToggleAsync

        await _repository.UpdateAsync(entity, cancellationToken);
        _logger.LogInformation("Updated Todo {TodoId}", entity.Id);
        return MapToDto(entity);
    }

    public async Task<TodoDto> ToggleAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Todo {TodoId} not found for toggle", id);
            throw new NotFoundException("Todo", id);
        }

        entity.IsCompleted = !entity.IsCompleted;
        await _repository.UpdateAsync(entity, cancellationToken);
        _logger.LogInformation("Toggled Todo {TodoId} IsCompleted to {IsCompleted}", entity.Id, entity.IsCompleted);
        return MapToDto(entity);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Todo {TodoId} not found for deletion", id);
            throw new NotFoundException("Todo", id);
        }

        await _repository.DeleteAsync(entity, cancellationToken);
        _logger.LogInformation("Deleted Todo {TodoId}", id);
    }

    private static TodoDto MapToDto(Todo entity) => new(
        entity.Id,
        entity.Title,
        entity.Description,
        entity.IsCompleted,
        entity.DueDate,
        entity.CreatedAt,
        entity.UpdatedAt
    );

    private static async Task ValidateAndThrowAsync<T>(
        IValidator<T> validator, T request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new Common.Exceptions.ValidationException(
                [.. validation.Errors.Select(e => e.ErrorMessage)]);
        }
    }
}
