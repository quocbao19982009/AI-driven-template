using Backend.Common.Exceptions;
using Backend.Common.Models;
using Backend.Data;
using Backend.Features.Factories;
using Backend.Features.Reservations;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ValidationException = Backend.Common.Exceptions.ValidationException;

namespace Backend.Tests.Features.Factories;

public class FactoriesServiceTests
{
    private readonly Mock<IFactoriesRepository> _repositoryMock;
    private readonly Mock<IValidator<CreateFactoryRequest>> _createValidatorMock;
    private readonly Mock<IValidator<UpdateFactoryRequest>> _updateValidatorMock;
    private readonly ApplicationDbContext _context;
    private readonly FactoriesService _sut;

    public FactoriesServiceTests()
    {
        _repositoryMock = new Mock<IFactoriesRepository>();
        _createValidatorMock = new Mock<IValidator<CreateFactoryRequest>>();
        _updateValidatorMock = new Mock<IValidator<UpdateFactoryRequest>>();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);

        _sut = new FactoriesService(
            _repositoryMock.Object,
            _context,
            _createValidatorMock.Object,
            _updateValidatorMock.Object,
            NullLogger<FactoriesService>.Instance);
    }

    // --- GetAllAsync ---

    [Fact]
    public async Task GetAllAsync_ValidParams_ReturnsPagedResult()
    {
        var entities = new List<Factory>
        {
            new() { Id = 1, Name = "Factory A", TimeZone = "UTC", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "Factory B", TimeZone = "UTC+2", CreatedAt = DateTime.UtcNow }
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

    // --- GetAllUnpagedAsync ---

    [Fact]
    public async Task GetAllUnpagedAsync_ReturnsAllEntities()
    {
        var entities = new List<Factory>
        {
            new() { Id = 1, Name = "Factory A", TimeZone = "UTC", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "Factory B", TimeZone = "UTC+2", CreatedAt = DateTime.UtcNow }
        };
        _repositoryMock
            .Setup(r => r.GetAllUnpagedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(entities);

        var result = await _sut.GetAllUnpagedAsync();

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Factory A");
    }

    // --- GetByIdAsync ---

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsDto()
    {
        var entity = new Factory { Id = 1, Name = "Factory A", TimeZone = "UTC", CreatedAt = DateTime.UtcNow };
        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _sut.GetByIdAsync(1);

        result.Id.Should().Be(1);
        result.Name.Should().Be("Factory A");
        result.TimeZone.Should().Be("UTC");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ThrowsNotFoundException()
    {
        _repositoryMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Factory?)null);

        var act = () => _sut.GetByIdAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedDto()
    {
        var request = new CreateFactoryRequest { Name = "New Factory", TimeZone = "UTC" };
        SetupValidValidator(_createValidatorMock, request);
        _repositoryMock
            .Setup(r => r.ExistsWithNameAsync(request.Name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Factory>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Factory e, CancellationToken _) => { e.Id = 1; return e; });

        var result = await _sut.CreateAsync(request);

        result.Id.Should().Be(1);
        result.Name.Should().Be("New Factory");
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Factory>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_InvalidRequest_ThrowsValidationException()
    {
        var request = new CreateFactoryRequest { Name = "" };
        SetupInvalidValidator(_createValidatorMock, request, "Name is required.");

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Factory>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ThrowsValidationException()
    {
        var request = new CreateFactoryRequest { Name = "Existing Factory", TimeZone = "UTC" };
        SetupValidValidator(_createValidatorMock, request);
        _repositoryMock
            .Setup(r => r.ExistsWithNameAsync(request.Name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Factory>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_SetsNameAndTimeZoneOnEntity()
    {
        var request = new CreateFactoryRequest { Name = "Test Factory", TimeZone = "America/New_York" };
        SetupValidValidator(_createValidatorMock, request);
        _repositoryMock
            .Setup(r => r.ExistsWithNameAsync(request.Name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        Factory? captured = null;
        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Factory>(), It.IsAny<CancellationToken>()))
            .Callback<Factory, CancellationToken>((e, _) => captured = e)
            .ReturnsAsync((Factory e, CancellationToken _) => e);

        await _sut.CreateAsync(request);

        captured!.Name.Should().Be("Test Factory");
        captured.TimeZone.Should().Be("America/New_York");
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_ValidRequest_ReturnsUpdatedDto()
    {
        var request = new UpdateFactoryRequest { Name = "Updated Factory", TimeZone = "UTC+1" };
        var entity = new Factory { Id = 1, Name = "Old Factory", TimeZone = "UTC", CreatedAt = DateTime.UtcNow };
        SetupValidValidator(_updateValidatorMock, request);
        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _repositoryMock
            .Setup(r => r.ExistsWithNameAsync(request.Name, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.UpdateAsync(1, request);

        result.Name.Should().Be("Updated Factory");
        result.TimeZone.Should().Be("UTC+1");
        _repositoryMock.Verify(r => r.UpdateAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingId_ThrowsNotFoundException()
    {
        var request = new UpdateFactoryRequest { Name = "Factory", TimeZone = "UTC" };
        SetupValidValidator(_updateValidatorMock, request);
        _repositoryMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Factory?)null);

        var act = () => _sut.UpdateAsync(999, request);

        await act.Should().ThrowAsync<NotFoundException>();
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Factory>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_InvalidRequest_ThrowsValidationException()
    {
        var request = new UpdateFactoryRequest { Name = "" };
        SetupInvalidValidator(_updateValidatorMock, request, "Name is required.");

        var act = () => _sut.UpdateAsync(1, request);

        await act.Should().ThrowAsync<ValidationException>();
        _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_DuplicateName_ThrowsValidationException()
    {
        var request = new UpdateFactoryRequest { Name = "Duplicate Factory", TimeZone = "UTC" };
        var entity = new Factory { Id = 1, Name = "Old Factory", TimeZone = "UTC", CreatedAt = DateTime.UtcNow };
        SetupValidValidator(_updateValidatorMock, request);
        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _repositoryMock
            .Setup(r => r.ExistsWithNameAsync(request.Name, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => _sut.UpdateAsync(1, request);

        await act.Should().ThrowAsync<ValidationException>();
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Factory>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_CascadesDisplayNameToReservations()
    {
        var request = new UpdateFactoryRequest { Name = "Renamed Factory", TimeZone = "UTC" };
        var entity = new Factory { Id = 1, Name = "Old Factory", TimeZone = "UTC", CreatedAt = DateTime.UtcNow };

        // Seed a reservation linked to this factory in the InMemory context
        var reservation = new Reservation
        {
            Id = 10,
            FactoryId = 1,
            FactoryDisplayName = "Old Factory",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(2)
        };
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        SetupValidValidator(_updateValidatorMock, request);
        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _repositoryMock
            .Setup(r => r.ExistsWithNameAsync(request.Name, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _sut.UpdateAsync(1, request);

        var updatedReservation = await _context.Reservations.FindAsync(10);
        updatedReservation!.FactoryDisplayName.Should().Be("Renamed Factory");
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_ExistingId_DeletesEntity()
    {
        var entity = new Factory { Id = 1, Name = "Factory", TimeZone = "UTC" };
        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        await _sut.DeleteAsync(1);

        _repositoryMock.Verify(r => r.DeleteAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingId_ThrowsNotFoundException()
    {
        _repositoryMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Factory?)null);

        var act = () => _sut.DeleteAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Factory>(), It.IsAny<CancellationToken>()), Times.Never);
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
