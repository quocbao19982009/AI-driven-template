using Backend.Common.Exceptions;
using Backend.Common.Models;
using Backend.Data;
using Backend.Features.Factories;
using Backend.Features.Personnel;
using Backend.Features.Reservations;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ValidationException = Backend.Common.Exceptions.ValidationException;

namespace Backend.Tests.Features.Reservations;

public class ReservationsServiceTests
{
    private readonly Mock<IReservationsRepository> _repositoryMock;
    private readonly Mock<IValidator<CreateReservationRequest>> _createValidatorMock;
    private readonly Mock<IValidator<UpdateReservationRequest>> _updateValidatorMock;
    private readonly ApplicationDbContext _context;
    private readonly ReservationsService _sut;

    private static readonly DateTime Start = new(2026, 6, 1, 8, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime End = new(2026, 6, 1, 16, 0, 0, DateTimeKind.Utc);

    public ReservationsServiceTests()
    {
        _repositoryMock = new Mock<IReservationsRepository>();
        _createValidatorMock = new Mock<IValidator<CreateReservationRequest>>();
        _updateValidatorMock = new Mock<IValidator<UpdateReservationRequest>>();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        _sut = new ReservationsService(
            _repositoryMock.Object,
            _context,
            _createValidatorMock.Object,
            _updateValidatorMock.Object,
            NullLogger<ReservationsService>.Instance);
    }

    // --- GetAllAsync ---

    [Fact]
    public async Task GetAllAsync_ValidParams_ReturnsPagedResult()
    {
        var entities = new List<Reservation>
        {
            new()
            {
                Id = 1, FactoryId = 1, FactoryDisplayName = "Factory A",
                StartTime = Start, EndTime = End, ReservationPersonnel = []
            }
        };
        _repositoryMock
            .Setup(r => r.GetAllAsync(1, 20, null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((entities, 1));

        var result = await _sut.GetAllAsync(1, 20, null, null, null, null);

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(-1, 20)]
    public async Task GetAllAsync_InvalidPage_ThrowsValidationException(int page, int pageSize)
    {
        var act = () => _sut.GetAllAsync(page, pageSize, null, null, null, null);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain(e => e.Contains("Page"));
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    [InlineData(1, 101)]
    public async Task GetAllAsync_InvalidPageSize_ThrowsValidationException(int page, int pageSize)
    {
        var act = () => _sut.GetAllAsync(page, pageSize, null, null, null, null);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain(e => e.Contains("Page size"));
    }

    // --- GetByIdAsync ---

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsDto()
    {
        var entity = new Reservation
        {
            Id = 1, FactoryId = 1, FactoryDisplayName = "Factory A",
            StartTime = Start, EndTime = End, ReservationPersonnel = []
        };
        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _sut.GetByIdAsync(1);

        result.Id.Should().Be(1);
        result.FactoryDisplayName.Should().Be("Factory A");
        result.DurationHours.Should().Be(8.0);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ThrowsNotFoundException()
    {
        _repositoryMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reservation?)null);

        var act = () => _sut.GetByIdAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedDto()
    {
        // Arrange: seed factory and person in InMemory context
        var factory = new Factory { Id = 1, Name = "Factory A", TimeZone = "UTC", CreatedAt = DateTime.UtcNow };
        _context.Factories.Add(factory);

        var person = new Person
        {
            Id = 1, PersonalId = "P001", FullName = "Alice Smith", Email = "alice@test.com",
            AllowedFactories = [factory], CreatedAt = DateTime.UtcNow
        };
        _context.Personnel.Add(person);
        await _context.SaveChangesAsync();

        var request = new CreateReservationRequest
        {
            FactoryId = 1, StartTime = Start, EndTime = End, PersonIds = [1]
        };
        SetupValidValidator(_createValidatorMock, request);

        _repositoryMock
            .Setup(r => r.HasOverlappingReservationAsync(It.IsAny<List<int>>(), Start, End, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var created = new Reservation
        {
            Id = 10, FactoryId = 1, FactoryDisplayName = "Factory A",
            StartTime = Start, EndTime = End,
            ReservationPersonnel = [new() { Id = 1, PersonId = 1, PersonDisplayName = "Alice Smith", ReservationId = 10 }]
        };
        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);
        _repositoryMock
            .Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(created);

        var result = await _sut.CreateAsync(request);

        result.Id.Should().Be(10);
        result.FactoryDisplayName.Should().Be("Factory A");
        result.Personnel.Should().HaveCount(1);
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_InvalidRequest_ThrowsValidationException()
    {
        var request = new CreateReservationRequest { FactoryId = 0, PersonIds = [] };
        SetupInvalidValidator(_createValidatorMock, request, "FactoryId is required.");

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_FactoryNotFound_ThrowsNotFoundException()
    {
        var request = new CreateReservationRequest
        {
            FactoryId = 999, StartTime = Start, EndTime = End, PersonIds = [1]
        };
        SetupValidValidator(_createValidatorMock, request);
        // Factory ID 999 not seeded in context

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<NotFoundException>();
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_PersonNotFound_ThrowsValidationException()
    {
        var factory = new Factory { Id = 1, Name = "Factory A", TimeZone = "UTC", CreatedAt = DateTime.UtcNow };
        _context.Factories.Add(factory);
        await _context.SaveChangesAsync();

        var request = new CreateReservationRequest
        {
            FactoryId = 1, StartTime = Start, EndTime = End, PersonIds = [999]
        };
        SetupValidValidator(_createValidatorMock, request);

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_PersonNotAllowedAtFactory_ThrowsValidationException()
    {
        var factory = new Factory { Id = 1, Name = "Factory A", TimeZone = "UTC", CreatedAt = DateTime.UtcNow };
        _context.Factories.Add(factory);

        // Person has no AllowedFactories — not assigned to Factory 1
        var person = new Person
        {
            Id = 1, PersonalId = "P001", FullName = "Alice Smith",
            Email = "alice@test.com", AllowedFactories = [], CreatedAt = DateTime.UtcNow
        };
        _context.Personnel.Add(person);
        await _context.SaveChangesAsync();

        var request = new CreateReservationRequest
        {
            FactoryId = 1, StartTime = Start, EndTime = End, PersonIds = [1]
        };
        SetupValidValidator(_createValidatorMock, request);

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_OverlappingReservation_ThrowsValidationException()
    {
        var factory = new Factory { Id = 1, Name = "Factory A", TimeZone = "UTC", CreatedAt = DateTime.UtcNow };
        _context.Factories.Add(factory);

        var person = new Person
        {
            Id = 1, PersonalId = "P001", FullName = "Alice Smith",
            Email = "alice@test.com", AllowedFactories = [factory], CreatedAt = DateTime.UtcNow
        };
        _context.Personnel.Add(person);
        await _context.SaveChangesAsync();

        var request = new CreateReservationRequest
        {
            FactoryId = 1, StartTime = Start, EndTime = End, PersonIds = [1]
        };
        SetupValidValidator(_createValidatorMock, request);
        _repositoryMock
            .Setup(r => r.HasOverlappingReservationAsync(It.IsAny<List<int>>(), Start, End, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_ValidRequest_ReturnsUpdatedDto()
    {
        var factory = new Factory { Id = 1, Name = "Factory A", TimeZone = "UTC", CreatedAt = DateTime.UtcNow };
        _context.Factories.Add(factory);

        var person = new Person
        {
            Id = 1, PersonalId = "P001", FullName = "Alice Smith",
            Email = "alice@test.com", AllowedFactories = [factory], CreatedAt = DateTime.UtcNow
        };
        _context.Personnel.Add(person);
        await _context.SaveChangesAsync();

        var existingEntity = new Reservation
        {
            Id = 1, FactoryId = 1, FactoryDisplayName = "Factory A",
            StartTime = Start, EndTime = End, ReservationPersonnel = []
        };
        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(existingEntity);

        var updatedEntity = new Reservation
        {
            Id = 1, FactoryId = 1, FactoryDisplayName = "Factory A",
            StartTime = Start, EndTime = End.AddHours(1),
            ReservationPersonnel = [new() { Id = 1, PersonId = 1, PersonDisplayName = "Alice Smith", ReservationId = 1 }]
        };
        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEntity)
            .Callback(() => _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(updatedEntity));

        var request = new UpdateReservationRequest
        {
            FactoryId = 1, StartTime = Start, EndTime = End.AddHours(1), PersonIds = [1]
        };
        SetupValidValidator(_updateValidatorMock, request);
        _repositoryMock
            .Setup(r => r.HasOverlappingReservationAsync(It.IsAny<List<int>>(), Start, End.AddHours(1), 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.UpdateAsync(1, request);

        result.Id.Should().Be(1);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingId_ThrowsNotFoundException()
    {
        var request = new UpdateReservationRequest
        {
            FactoryId = 1, StartTime = Start, EndTime = End, PersonIds = [1]
        };
        SetupValidValidator(_updateValidatorMock, request);
        _repositoryMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Reservation?)null);

        var act = () => _sut.UpdateAsync(999, request);

        await act.Should().ThrowAsync<NotFoundException>();
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_InvalidRequest_ThrowsValidationException()
    {
        var request = new UpdateReservationRequest { FactoryId = 0, PersonIds = [] };
        SetupInvalidValidator(_updateValidatorMock, request, "FactoryId is required.");

        var act = () => _sut.UpdateAsync(1, request);

        await act.Should().ThrowAsync<ValidationException>();
        _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_ExistingId_DeletesEntity()
    {
        var entity = new Reservation
        {
            Id = 1, FactoryId = 1, FactoryDisplayName = "Factory A",
            StartTime = Start, EndTime = End, ReservationPersonnel = []
        };
        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        await _sut.DeleteAsync(1);

        _repositoryMock.Verify(r => r.DeleteAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingId_ThrowsNotFoundException()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Reservation?)null);

        var act = () => _sut.DeleteAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // --- MapToDto ---

    [Fact]
    public async Task GetByIdAsync_DurationHoursCalculatedCorrectly()
    {
        var start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = new DateTime(2026, 1, 1, 4, 30, 0, DateTimeKind.Utc);
        var entity = new Reservation
        {
            Id = 1, FactoryId = 1, FactoryDisplayName = "F",
            StartTime = start, EndTime = end, ReservationPersonnel = []
        };
        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        var result = await _sut.GetByIdAsync(1);

        result.DurationHours.Should().Be(4.5);
    }

    // --- Helpers ---

    private static void SetupValidValidator<T>(Mock<IValidator<T>> mock, T request)
    {
        mock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
    }

    private static void SetupInvalidValidator<T>(Mock<IValidator<T>> mock, T request, string errorMessage)
    {
        var failures = new List<ValidationFailure> { new("Property", errorMessage) };
        mock.Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));
    }
}
