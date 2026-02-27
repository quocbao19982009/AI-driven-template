using Backend.Common.Exceptions;
using Backend.Common.Models;
using Backend.Data;
using Backend.Features.Factories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Personnel;

public class PersonnelService : IPersonnelService
{
    private readonly IPersonnelRepository _repository;
    private readonly IFactoriesRepository _factoriesRepository;
    private readonly ApplicationDbContext _context;
    private readonly IValidator<CreatePersonRequest> _createValidator;
    private readonly IValidator<UpdatePersonRequest> _updateValidator;
    private readonly ILogger<PersonnelService> _logger;

    public PersonnelService(
        IPersonnelRepository repository,
        IFactoriesRepository factoriesRepository,
        ApplicationDbContext context,
        IValidator<CreatePersonRequest> createValidator,
        IValidator<UpdatePersonRequest> updateValidator,
        ILogger<PersonnelService> logger)
    {
        _repository = repository;
        _factoriesRepository = factoriesRepository;
        _context = context;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<PagedResult<PersonDto>> GetAllAsync(
        int page, int pageSize, string? search, CancellationToken cancellationToken = default)
    {
        List<string> errors = [];
        if (page < 1) errors.Add("Page must be greater than or equal to 1.");
        if (pageSize < 1 || pageSize > 100) errors.Add("Page size must be between 1 and 100.");
        if (errors.Count > 0) throw new Common.Exceptions.ValidationException(errors);

        var (entities, totalCount) = await _repository.GetAllAsync(page, pageSize, search, cancellationToken);
        var items = entities.Select(MapToDto).ToList();
        return new PagedResult<PersonDto>(items, totalCount, page, pageSize);
    }

    public async Task<List<PersonDto>> GetAllUnpagedAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllUnpagedAsync(cancellationToken);
        return entities.Select(MapToDto).ToList();
    }

    public async Task<PersonDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Person {PersonId} not found", id);
            throw new NotFoundException("Person", id);
        }
        return MapToDto(entity);
    }

    public async Task<PersonDto> CreateAsync(CreatePersonRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAndThrowAsync(_createValidator, request, cancellationToken);

        if (await _repository.ExistsWithPersonalIdAsync(request.PersonalId, cancellationToken: cancellationToken))
            throw new Common.Exceptions.ValidationException([$"A person with PersonalId '{request.PersonalId}' already exists."]);

        if (await _repository.ExistsWithEmailAsync(request.Email, cancellationToken: cancellationToken))
            throw new Common.Exceptions.ValidationException([$"A person with email '{request.Email}' already exists."]);

        var allowedFactories = await ResolveFactoriesAsync(request.AllowedFactoryIds, cancellationToken);

        var entity = new Person
        {
            PersonalId = request.PersonalId,
            FullName = request.FullName,
            Email = request.Email,
            AllowedFactories = allowedFactories
        };

        var created = await _repository.CreateAsync(entity, cancellationToken);
        _logger.LogInformation("Created Person {PersonId} with name {FullName}", created.Id, created.FullName);
        return MapToDto(created);
    }

    public async Task<PersonDto> UpdateAsync(int id, UpdatePersonRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAndThrowAsync(_updateValidator, request, cancellationToken);

        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Person {PersonId} not found for update", id);
            throw new NotFoundException("Person", id);
        }

        if (await _repository.ExistsWithPersonalIdAsync(request.PersonalId, excludeId: id, cancellationToken: cancellationToken))
            throw new Common.Exceptions.ValidationException([$"A person with PersonalId '{request.PersonalId}' already exists."]);

        if (await _repository.ExistsWithEmailAsync(request.Email, excludeId: id, cancellationToken: cancellationToken))
            throw new Common.Exceptions.ValidationException([$"A person with email '{request.Email}' already exists."]);

        var allowedFactories = await ResolveFactoriesAsync(request.AllowedFactoryIds, cancellationToken);

        entity.PersonalId = request.PersonalId;
        entity.FullName = request.FullName;
        entity.Email = request.Email;
        entity.AllowedFactories = allowedFactories;

        await _repository.UpdateAsync(entity, cancellationToken);

        // Cascade name update to all reservation-person entries linked to this person
        var reservationPersonnel = await _context.ReservationPersonnel
            .Where(rp => rp.PersonId == entity.Id)
            .ToListAsync(cancellationToken);

        foreach (var rp in reservationPersonnel)
            rp.PersonDisplayName = request.FullName;

        if (reservationPersonnel.Count > 0)
            await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated Person {PersonId}", entity.Id);
        return MapToDto(entity);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Person {PersonId} not found for deletion", id);
            throw new NotFoundException("Person", id);
        }

        // Soft-delete: null out PersonId on associated ReservationPerson records, keep display name
        var reservationPersonnel = await _context.ReservationPersonnel
            .Where(rp => rp.PersonId == id)
            .ToListAsync(cancellationToken);

        foreach (var rp in reservationPersonnel)
        {
            rp.PersonId = null;
        }

        await _context.SaveChangesAsync(cancellationToken);
        await _repository.DeleteAsync(entity, cancellationToken);
        _logger.LogInformation("Deleted Person {PersonId}, nulled {Count} ReservationPerson records", id, reservationPersonnel.Count);
    }

    private async Task<List<Factory>> ResolveFactoriesAsync(List<int> factoryIds, CancellationToken cancellationToken)
    {
        if (factoryIds.Count == 0) return [];

        var factories = await _context.Factories
            .Where(f => factoryIds.Contains(f.Id))
            .ToListAsync(cancellationToken);

        var missing = factoryIds.Except(factories.Select(f => f.Id)).ToList();
        if (missing.Count > 0)
            throw new Common.Exceptions.ValidationException([$"Factory IDs not found: {string.Join(", ", missing)}"]);

        return factories;
    }

    internal static PersonDto MapToDto(Person entity) => new(
        entity.Id,
        entity.PersonalId,
        entity.FullName,
        entity.Email,
        entity.AllowedFactories.Select(FactoriesService.MapToDto).ToList(),
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
