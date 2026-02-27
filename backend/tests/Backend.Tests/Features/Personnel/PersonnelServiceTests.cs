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

namespace Backend.Tests.Features.Personnel;

public class PersonnelServiceTests
{
    private readonly Mock<IPersonnelRepository> _repositoryMock;
    private readonly Mock<IFactoriesRepository> _factoriesRepositoryMock;
    private readonly Mock<IValidator<CreatePersonRequest>> _createValidatorMock;
    private readonly Mock<IValidator<UpdatePersonRequest>> _updateValidatorMock;
    private readonly ApplicationDbContext _context;
    private readonly PersonnelService _sut;

    public PersonnelServiceTests()
    {
        _repositoryMock = new Mock<IPersonnelRepository>();
        _factoriesRepositoryMock = new Mock<IFactoriesRepository>();
        _createValidatorMock = new Mock<IValidator<CreatePersonRequest>>();
        _updateValidatorMock = new Mock<IValidator<UpdatePersonRequest>>();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        _sut = new PersonnelService(
            _repositoryMock.Object,
            _factoriesRepositoryMock.Object,
            _context,
            _createValidatorMock.Object,
            _updateValidatorMock.Object,
            NullLogger<PersonnelService>.Instance);
    }

    // --- GetAllAsync ---

    [Fact]
    public async Task GetAllAsync_ValidParams_ReturnsPagedResult()
    {
        var entities = new List<Person>
        {
            new() { Id = 1, PersonalId = "P001", FullName = "Alice Smith", Email = "alice@test.com", AllowedFactories = [], CreatedAt = DateTime.UtcNow },
            new() { Id = 2, PersonalId = "P002", FullName = "Bob Jones", Email = "bob@test.com", AllowedFactories = [], CreatedAt = DateTime.UtcNow }
        };
        _repositoryMock
            .Setup(r => r.GetAllAsync(1, 20, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((entities, 2));

        var result = await _sut.GetAllAsync(1, 20, null);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(-1, 20)]
    public async Task GetAllAsync_InvalidPage_ThrowsValidationException(int page, int pageSize)
    {
        var act = () => _sut.GetAllAsync(page, pageSize, null);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain(e => e.Contains("Page"));
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    [InlineData(1, 101)]
    public async Task GetAllAsync_InvalidPageSize_ThrowsValidationException(int page, int pageSize)
    {
        var act = () => _sut.GetAllAsync(page, pageSize, null);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain(e => e.Contains("Page size"));
    }

    // --- GetByIdAsync ---

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsDto()
    {
        var entity = new Person
        {
            Id = 1, PersonalId = "P001", FullName = "Alice Smith",
            Email = "alice@test.com", AllowedFactories = [], CreatedAt = DateTime.UtcNow
        };
        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _sut.GetByIdAsync(1);

        result.Id.Should().Be(1);
        result.FullName.Should().Be("Alice Smith");
        result.PersonalId.Should().Be("P001");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ThrowsNotFoundException()
    {
        _repositoryMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person?)null);

        var act = () => _sut.GetByIdAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedDto()
    {
        var request = new CreatePersonRequest
        {
            PersonalId = "P001", FullName = "Alice Smith",
            Email = "alice@test.com", AllowedFactoryIds = []
        };
        SetupValidValidator(_createValidatorMock, request);
        _repositoryMock.Setup(r => r.ExistsWithPersonalIdAsync(request.PersonalId, null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.ExistsWithEmailAsync(request.Email, null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person e, CancellationToken _) => { e.Id = 1; return e; });

        var result = await _sut.CreateAsync(request);

        result.Id.Should().Be(1);
        result.FullName.Should().Be("Alice Smith");
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_InvalidRequest_ThrowsValidationException()
    {
        var request = new CreatePersonRequest { PersonalId = "", FullName = "", Email = "" };
        SetupInvalidValidator(_createValidatorMock, request, "PersonalId is required.");

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_DuplicatePersonalId_ThrowsValidationException()
    {
        var request = new CreatePersonRequest
        {
            PersonalId = "P001", FullName = "Alice Smith",
            Email = "alice@test.com", AllowedFactoryIds = []
        };
        SetupValidValidator(_createValidatorMock, request);
        _repositoryMock.Setup(r => r.ExistsWithPersonalIdAsync(request.PersonalId, null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_DuplicateEmail_ThrowsValidationException()
    {
        var request = new CreatePersonRequest
        {
            PersonalId = "P001", FullName = "Alice Smith",
            Email = "alice@test.com", AllowedFactoryIds = []
        };
        SetupValidValidator(_createValidatorMock, request);
        _repositoryMock.Setup(r => r.ExistsWithPersonalIdAsync(request.PersonalId, null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.ExistsWithEmailAsync(request.Email, null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_UnknownFactoryId_ThrowsValidationException()
    {
        var request = new CreatePersonRequest
        {
            PersonalId = "P001", FullName = "Alice Smith",
            Email = "alice@test.com", AllowedFactoryIds = [999]
        };
        SetupValidValidator(_createValidatorMock, request);
        _repositoryMock.Setup(r => r.ExistsWithPersonalIdAsync(request.PersonalId, null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.ExistsWithEmailAsync(request.Email, null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        // Factory ID 999 is not in the InMemory context

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithValidFactoryIds_ResolvesFactoriesFromContext()
    {
        var factory = new Factory { Id = 1, Name = "Factory A", TimeZone = "UTC", CreatedAt = DateTime.UtcNow };
        _context.Factories.Add(factory);
        await _context.SaveChangesAsync();

        var request = new CreatePersonRequest
        {
            PersonalId = "P001", FullName = "Alice Smith",
            Email = "alice@test.com", AllowedFactoryIds = [1]
        };
        SetupValidValidator(_createValidatorMock, request);
        _repositoryMock.Setup(r => r.ExistsWithPersonalIdAsync(request.PersonalId, null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.ExistsWithEmailAsync(request.Email, null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        Person? captured = null;
        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()))
            .Callback<Person, CancellationToken>((e, _) => captured = e)
            .ReturnsAsync((Person e, CancellationToken _) => { e.Id = 2; return e; });

        await _sut.CreateAsync(request);

        captured!.AllowedFactories.Should().ContainSingle(f => f.Id == 1);
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_ValidRequest_ReturnsUpdatedDto()
    {
        var request = new UpdatePersonRequest
        {
            PersonalId = "P001-U", FullName = "Alice Updated",
            Email = "alice.updated@test.com", AllowedFactoryIds = []
        };
        var entity = new Person
        {
            Id = 1, PersonalId = "P001", FullName = "Alice Smith",
            Email = "alice@test.com", AllowedFactories = [], CreatedAt = DateTime.UtcNow
        };
        SetupValidValidator(_updateValidatorMock, request);
        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _repositoryMock.Setup(r => r.ExistsWithPersonalIdAsync(request.PersonalId, 1, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.ExistsWithEmailAsync(request.Email, 1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _sut.UpdateAsync(1, request);

        result.FullName.Should().Be("Alice Updated");
        result.PersonalId.Should().Be("P001-U");
        _repositoryMock.Verify(r => r.UpdateAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingId_ThrowsNotFoundException()
    {
        var request = new UpdatePersonRequest
        {
            PersonalId = "P001", FullName = "Alice", Email = "alice@test.com", AllowedFactoryIds = []
        };
        SetupValidValidator(_updateValidatorMock, request);
        _repositoryMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Person?)null);

        var act = () => _sut.UpdateAsync(999, request);

        await act.Should().ThrowAsync<NotFoundException>();
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_InvalidRequest_ThrowsValidationException()
    {
        var request = new UpdatePersonRequest { PersonalId = "", FullName = "", Email = "" };
        SetupInvalidValidator(_updateValidatorMock, request, "PersonalId is required.");

        var act = () => _sut.UpdateAsync(1, request);

        await act.Should().ThrowAsync<ValidationException>();
        _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_CascadesDisplayNameToReservationPersonnel()
    {
        var entity = new Person
        {
            Id = 1, PersonalId = "P001", FullName = "Alice Smith",
            Email = "alice@test.com", AllowedFactories = [], CreatedAt = DateTime.UtcNow
        };

        // Seed reservation person record in the InMemory context
        var reservation = new Reservation
        {
            Id = 10,
            FactoryId = null,
            FactoryDisplayName = "Factory A",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(2)
        };
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        var rp = new ReservationPerson
        {
            Id = 20,
            ReservationId = 10,
            PersonId = 1,
            PersonDisplayName = "Alice Smith"
        };
        _context.ReservationPersonnel.Add(rp);
        await _context.SaveChangesAsync();

        var request = new UpdatePersonRequest
        {
            PersonalId = "P001", FullName = "Alice Renamed",
            Email = "alice@test.com", AllowedFactoryIds = []
        };
        SetupValidValidator(_updateValidatorMock, request);
        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
        _repositoryMock.Setup(r => r.ExistsWithPersonalIdAsync(request.PersonalId, 1, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _repositoryMock.Setup(r => r.ExistsWithEmailAsync(request.Email, 1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await _sut.UpdateAsync(1, request);

        var updatedRp = await _context.ReservationPersonnel.FindAsync(20);
        updatedRp!.PersonDisplayName.Should().Be("Alice Renamed");
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_ExistingId_DeletesEntityAndNullsReservationPersonnel()
    {
        var entity = new Person
        {
            Id = 1, PersonalId = "P001", FullName = "Alice Smith",
            Email = "alice@test.com", AllowedFactories = []
        };

        var reservation = new Reservation
        {
            Id = 10, FactoryId = null, FactoryDisplayName = "Factory",
            StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(2)
        };
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        var rp = new ReservationPerson
        {
            Id = 20, ReservationId = 10, PersonId = 1, PersonDisplayName = "Alice Smith"
        };
        _context.ReservationPersonnel.Add(rp);
        await _context.SaveChangesAsync();

        _repositoryMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(entity);

        await _sut.DeleteAsync(1);

        var updatedRp = await _context.ReservationPersonnel.FindAsync(20);
        updatedRp!.PersonId.Should().BeNull();
        _repositoryMock.Verify(r => r.DeleteAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingId_ThrowsNotFoundException()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Person?)null);

        var act = () => _sut.DeleteAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()), Times.Never);
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
