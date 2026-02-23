using Backend.Common.Exceptions;
using Backend.Features._FeatureTemplate;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ValidationException = Backend.Common.Exceptions.ValidationException;

namespace Backend.Tests.Features._FeatureTemplate;

public class FeatureServiceTests
{
    private readonly Mock<IFeatureRepository> _repositoryMock;
    private readonly Mock<IValidator<CreateFeatureRequest>> _createValidatorMock;
    private readonly Mock<IValidator<UpdateFeatureRequest>> _updateValidatorMock;
    private readonly FeatureService _sut;

    public FeatureServiceTests()
    {
        _repositoryMock = new Mock<IFeatureRepository>();
        _createValidatorMock = new Mock<IValidator<CreateFeatureRequest>>();
        _updateValidatorMock = new Mock<IValidator<UpdateFeatureRequest>>();

        _sut = new FeatureService(
            _repositoryMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object,
            NullLogger<FeatureService>.Instance);
    }

    // --- GetAllAsync ---

    [Fact]
    public async Task GetAllAsync_ValidParams_ReturnsPagedResult()
    {
        var entities = new List<Feature>
        {
            new() { Id = 1, Name = "Item 1", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "Item 2", CreatedAt = DateTime.UtcNow }
        };
        _repositoryMock
            .Setup(r => r.GetAllAsync(1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((entities, 2));

        var result = await _sut.GetAllAsync(1, 20);

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
        var act = () => _sut.GetAllAsync(page, pageSize);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain(e => e.Contains("Page"));
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    [InlineData(1, 101)]
    public async Task GetAllAsync_InvalidPageSize_ThrowsValidationException(int page, int pageSize)
    {
        var act = () => _sut.GetAllAsync(page, pageSize);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain(e => e.Contains("Page size"));
    }

    // --- GetByIdAsync ---

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsDto()
    {
        var entity = new Feature { Id = 1, Name = "Test", CreatedAt = DateTime.UtcNow };
        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _sut.GetByIdAsync(1);

        result.Id.Should().Be(1);
        result.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ThrowsNotFoundException()
    {
        _repositoryMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Feature?)null);

        var act = () => _sut.GetByIdAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedDto()
    {
        var request = new CreateFeatureRequest { Name = "New Item" };
        SetupValidValidator(_createValidatorMock, request);

        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Feature>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Feature e, CancellationToken _) =>
            {
                e.Id = 1;
                return e;
            });

        var result = await _sut.CreateAsync(request);

        result.Id.Should().Be(1);
        result.Name.Should().Be("New Item");
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Feature>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_InvalidRequest_ThrowsValidationException()
    {
        var request = new CreateFeatureRequest { Name = "" };
        SetupInvalidValidator(_createValidatorMock, request, "Name is required.");

        var act = () => _sut.CreateAsync(request);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain("Name is required.");
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Feature>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_ValidRequest_ReturnsUpdatedDto()
    {
        var request = new UpdateFeatureRequest { Name = "Updated" };
        var entity = new Feature { Id = 1, Name = "Old", CreatedAt = DateTime.UtcNow };

        SetupValidValidator(_updateValidatorMock, request);
        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _sut.UpdateAsync(1, request);

        result.Name.Should().Be("Updated");
        _repositoryMock.Verify(r => r.UpdateAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingId_ThrowsNotFoundException()
    {
        var request = new UpdateFeatureRequest { Name = "Updated" };
        SetupValidValidator(_updateValidatorMock, request);
        _repositoryMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Feature?)null);

        var act = () => _sut.UpdateAsync(999, request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_InvalidRequest_ThrowsValidationException()
    {
        var request = new UpdateFeatureRequest { Name = "" };
        SetupInvalidValidator(_updateValidatorMock, request, "Name is required.");

        var act = () => _sut.UpdateAsync(1, request);

        await act.Should().ThrowAsync<ValidationException>();
        _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_ExistingId_DeletesEntity()
    {
        var entity = new Feature { Id = 1, Name = "Test" };
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
            .ReturnsAsync((Feature?)null);

        var act = () => _sut.DeleteAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
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
