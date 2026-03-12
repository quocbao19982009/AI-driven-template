using Backend.Common.Exceptions;
using Backend.Common.Models;
using FluentValidation;

namespace Backend.Features.Users;

public class UsersService : IUsersService
{
    private readonly IUsersRepository _repository;
    private readonly IValidator<CreateUserRequest> _createValidator;
    private readonly IValidator<UpdateUserRequest> _updateValidator;
    private readonly ILogger<UsersService> _logger;

    public UsersService(
        IUsersRepository repository,
        IValidator<CreateUserRequest> createValidator,
        IValidator<UpdateUserRequest> updateValidator,
        ILogger<UsersService> logger)
    {
        _repository = repository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<PagedResult<UserDto>> GetAllAsync(
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
        _logger.LogDebug("Retrieved {Count} of {Total} Users (page {Page})", items.Count, totalCount, page);
        return new PagedResult<UserDto>(items, totalCount, page, pageSize);
    }

    public async Task<UserDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("User {UserId} not found", id);
            throw new NotFoundException("User", id);
        }

        return MapToDto(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAndThrowAsync(_createValidator, request, cancellationToken);

        if (await _repository.EmailExistsAsync(request.Email, cancellationToken: cancellationToken))
        {
            throw new Common.Exceptions.ValidationException(
                ["A user with this email already exists."]);
        }

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = BCryptHash(request.Password),
            Role = request.Role ?? "User"
        };

        var created = await _repository.CreateAsync(user, cancellationToken);
        _logger.LogInformation("Created User {UserId} with email {UserEmail}", created.Id, created.Email);
        return MapToDto(created);
    }

    public async Task<UserDto> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAndThrowAsync(_updateValidator, request, cancellationToken);

        var user = await _repository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("User {UserId} not found for update", id);
            throw new NotFoundException("User", id);
        }

        if (await _repository.EmailExistsAsync(request.Email, id, cancellationToken))
        {
            throw new Common.Exceptions.ValidationException(
                ["A user with this email already exists."]);
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.Role = request.Role ?? user.Role;

        await _repository.UpdateAsync(user, cancellationToken);
        _logger.LogInformation("Updated User {UserId}", user.Id);
        return MapToDto(user);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("User {UserId} not found for deletion", id);
            throw new NotFoundException("User", id);
        }

        await _repository.DeleteAsync(user, cancellationToken);
        _logger.LogInformation("Deleted User {UserId}", id);
    }

    private static UserDto MapToDto(User user) => new(
        user.Id,
        user.FirstName,
        user.LastName,
        user.Email,
        user.Role,
        user.IsActive,
        user.CreatedAt
    );

    private static string BCryptHash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

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
