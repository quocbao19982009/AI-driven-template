using Backend.Common.Exceptions;
using Backend.Common.Models;
using Backend.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Reservations;

public class ReservationsService : IReservationsService
{
    private readonly IReservationsRepository _repository;
    private readonly ApplicationDbContext _context;
    private readonly IValidator<CreateReservationRequest> _createValidator;
    private readonly IValidator<UpdateReservationRequest> _updateValidator;
    private readonly ILogger<ReservationsService> _logger;

    public ReservationsService(
        IReservationsRepository repository,
        ApplicationDbContext context,
        IValidator<CreateReservationRequest> createValidator,
        IValidator<UpdateReservationRequest> updateValidator,
        ILogger<ReservationsService> logger)
    {
        _repository = repository;
        _context = context;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _logger = logger;
    }

    public async Task<PagedResult<ReservationDto>> GetAllAsync(
        int page, int pageSize,
        int? factoryId, int? personId,
        DateTime? fromDate, DateTime? toDate,
        CancellationToken cancellationToken = default)
    {
        List<string> errors = [];
        if (page < 1) errors.Add("Page must be greater than or equal to 1.");
        if (pageSize < 1 || pageSize > 100) errors.Add("Page size must be between 1 and 100.");
        if (errors.Count > 0) throw new Common.Exceptions.ValidationException(errors);

        var (entities, totalCount) = await _repository.GetAllAsync(
            page, pageSize, factoryId, personId, fromDate, toDate, cancellationToken);
        var items = entities.Select(MapToDto).ToList();
        return new PagedResult<ReservationDto>(items, totalCount, page, pageSize);
    }

    public async Task<ReservationDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Reservation {ReservationId} not found", id);
            throw new NotFoundException("Reservation", id);
        }
        return MapToDto(entity);
    }

    public async Task<ReservationDto> CreateAsync(CreateReservationRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAndThrowAsync(_createValidator, request, cancellationToken);

        var factory = await _context.Factories.FindAsync([request.FactoryId], cancellationToken);
        if (factory is null)
            throw new NotFoundException("Factory", request.FactoryId);

        // Resolve persons and validate factory membership
        var persons = await _context.Personnel
            .Include(p => p.AllowedFactories)
            .Where(p => request.PersonIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        var missingPersons = request.PersonIds.Except(persons.Select(p => p.Id)).ToList();
        if (missingPersons.Count > 0)
            throw new Common.Exceptions.ValidationException([$"Person IDs not found: {string.Join(", ", missingPersons)}"]);

        var notAllowed = persons
            .Where(p => !p.AllowedFactories.Any(f => f.Id == request.FactoryId))
            .Select(p => p.FullName)
            .ToList();

        if (notAllowed.Count > 0)
            throw new Common.Exceptions.ValidationException(
                [$"The following persons are not allowed at factory '{factory.Name}': {string.Join(", ", notAllowed)}"]);

        // Overlap check
        if (await _repository.HasOverlappingReservationAsync(
            request.PersonIds, request.StartTime, request.EndTime, cancellationToken: cancellationToken))
        {
            throw new Common.Exceptions.ValidationException(
                ["One or more persons already have a reservation during the requested time slot."]);
        }

        var entity = new Reservation
        {
            FactoryId = request.FactoryId,
            FactoryDisplayName = factory.Name,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            ReservationPersonnel = persons.Select(p => new ReservationPerson
            {
                PersonId = p.Id,
                PersonDisplayName = p.FullName
            }).ToList()
        };

        var created = await _repository.CreateAsync(entity, cancellationToken);
        _logger.LogInformation("Created Reservation {ReservationId}", created.Id);

        // Reload with personnel to return full DTO
        var reloaded = await _repository.GetByIdAsync(created.Id, cancellationToken);
        return MapToDto(reloaded!);
    }

    public async Task<ReservationDto> UpdateAsync(int id, UpdateReservationRequest request, CancellationToken cancellationToken = default)
    {
        await ValidateAndThrowAsync(_updateValidator, request, cancellationToken);

        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Reservation {ReservationId} not found for update", id);
            throw new NotFoundException("Reservation", id);
        }

        var factory = await _context.Factories.FindAsync([request.FactoryId], cancellationToken);
        if (factory is null)
            throw new NotFoundException("Factory", request.FactoryId);

        var persons = await _context.Personnel
            .Include(p => p.AllowedFactories)
            .Where(p => request.PersonIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        var missingPersons = request.PersonIds.Except(persons.Select(p => p.Id)).ToList();
        if (missingPersons.Count > 0)
            throw new Common.Exceptions.ValidationException([$"Person IDs not found: {string.Join(", ", missingPersons)}"]);

        var notAllowed = persons
            .Where(p => !p.AllowedFactories.Any(f => f.Id == request.FactoryId))
            .Select(p => p.FullName)
            .ToList();

        if (notAllowed.Count > 0)
            throw new Common.Exceptions.ValidationException(
                [$"The following persons are not allowed at factory '{factory.Name}': {string.Join(", ", notAllowed)}"]);

        if (await _repository.HasOverlappingReservationAsync(
            request.PersonIds, request.StartTime, request.EndTime, excludeReservationId: id, cancellationToken: cancellationToken))
        {
            throw new Common.Exceptions.ValidationException(
                ["One or more persons already have a reservation during the requested time slot."]);
        }

        // Remove old personnel and replace
        entity.ReservationPersonnel.Clear();
        entity.FactoryId = request.FactoryId;
        entity.FactoryDisplayName = factory.Name;
        entity.StartTime = request.StartTime;
        entity.EndTime = request.EndTime;
        entity.ReservationPersonnel = persons.Select(p => new ReservationPerson
        {
            PersonId = p.Id,
            PersonDisplayName = p.FullName
        }).ToList();

        await _repository.UpdateAsync(entity, cancellationToken);
        _logger.LogInformation("Updated Reservation {ReservationId}", entity.Id);

        var reloaded = await _repository.GetByIdAsync(id, cancellationToken);
        return MapToDto(reloaded!);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity is null)
        {
            _logger.LogWarning("Reservation {ReservationId} not found for deletion", id);
            throw new NotFoundException("Reservation", id);
        }

        await _repository.DeleteAsync(entity, cancellationToken);
        _logger.LogInformation("Deleted Reservation {ReservationId}", id);
    }

    internal static ReservationDto MapToDto(Reservation entity) => new(
        entity.Id,
        entity.FactoryId,
        entity.FactoryDisplayName,
        entity.StartTime,
        entity.EndTime,
        (entity.EndTime - entity.StartTime).TotalHours,
        entity.ReservationPersonnel.Select(rp => new ReservationPersonDto(
            rp.Id, rp.PersonId, rp.PersonDisplayName)).ToList(),
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
