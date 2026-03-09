using Backend.Common.Exceptions;
using Backend.Features.Todos;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ValidationException = Backend.Common.Exceptions.ValidationException;

namespace Backend.Tests.Features.Todos;

public class TodoServiceTests
{
    private readonly Mock<ITodosRepository> _repositoryMock;
    private readonly Mock<IValidator<CreateTodoRequest>> _createValidatorMock;
    private readonly Mock<IValidator<UpdateTodoRequest>> _updateValidatorMock;
    private readonly TodosService _sut;

    public TodoServiceTests()
    {
        _repositoryMock = new Mock<ITodosRepository>();
        _createValidatorMock = new Mock<IValidator<CreateTodoRequest>>();
        _updateValidatorMock = new Mock<IValidator<UpdateTodoRequest>>();

        _sut = new TodosService(
            _repositoryMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object,
            NullLogger<TodosService>.Instance);
    }

    // --- GetAllAsync ---

    [Fact]
    public async Task GetAllAsync_ValidParams_ReturnsPagedResult()
    {
        var entities = new List<Todo>
        {
            new() { Id = 1, Title = "Todo 1", IsCompleted = false, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, Title = "Todo 2", IsCompleted = true, CreatedAt = DateTime.UtcNow }
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
        var entity = new Todo { Id = 1, Title = "Test", IsCompleted = false, CreatedAt = DateTime.UtcNow };
        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _sut.GetByIdAsync(1);

        result.Id.Should().Be(1);
        result.Title.Should().Be("Test");
        result.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ThrowsNotFoundException()
    {
        _repositoryMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Todo?)null);

        var act = () => _sut.GetByIdAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsTodoDto()
    {
        var request = new CreateTodoRequest { Title = "Buy milk" };
        SetupValidValidator(_createValidatorMock, request);

        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Todo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Todo e, CancellationToken _) =>
            {
                e.Id = 1;
                return e;
            });

        var result = await _sut.CreateAsync(request);

        result.Id.Should().Be(1);
        result.Title.Should().Be("Buy milk");
        result.IsCompleted.Should().BeFalse();
        result.DueDate.Should().BeNull();
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Todo>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithEmptyTitle_ThrowsValidationException()
    {
        var request = new CreateTodoRequest { Title = "" };
        SetupInvalidValidator(_createValidatorMock, request, "Title is required.");

        var act = () => _sut.CreateAsync(request);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain("Title is required.");
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Todo>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithPastDueDate_ThrowsValidationException()
    {
        var request = new CreateTodoRequest { Title = "Old task", DueDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc) };
        SetupInvalidValidator(_createValidatorMock, request, "DueDate must be in the future.");

        var act = () => _sut.CreateAsync(request);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain("DueDate must be in the future.");
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_ValidRequest_ReturnsUpdatedDto()
    {
        var request = new UpdateTodoRequest { Title = "Updated title" };
        var entity = new Todo { Id = 1, Title = "Old title", IsCompleted = false, CreatedAt = DateTime.UtcNow };

        SetupValidValidator(_updateValidatorMock, request);
        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _sut.UpdateAsync(1, request);

        result.Title.Should().Be("Updated title");
        result.IsCompleted.Should().BeFalse(); // IsCompleted unchanged
        _repositoryMock.Verify(r => r.UpdateAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingId_ThrowsNotFoundException()
    {
        var request = new UpdateTodoRequest { Title = "Updated" };
        SetupValidValidator(_updateValidatorMock, request);
        _repositoryMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Todo?)null);

        var act = () => _sut.UpdateAsync(999, request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // --- ToggleAsync ---

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted_FalseToTrue()
    {
        var entity = new Todo { Id = 1, Title = "Test", IsCompleted = false, CreatedAt = DateTime.UtcNow };
        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _sut.ToggleAsync(1);

        result.IsCompleted.Should().BeTrue();
        _repositoryMock.Verify(r => r.UpdateAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ToggleAsync_FlipsIsCompleted_TrueToFalse()
    {
        var entity = new Todo { Id = 1, Title = "Test", IsCompleted = true, CreatedAt = DateTime.UtcNow };
        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _sut.ToggleAsync(1);

        result.IsCompleted.Should().BeFalse();
        _repositoryMock.Verify(r => r.UpdateAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ToggleAsync_WithInvalidId_ThrowsNotFoundException()
    {
        _repositoryMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Todo?)null);

        var act = () => _sut.ToggleAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_ExistingId_DeletesEntity()
    {
        var entity = new Todo { Id = 1, Title = "Test" };
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
            .ReturnsAsync((Todo?)null);

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
