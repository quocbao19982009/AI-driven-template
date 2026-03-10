using Backend.Common.Exceptions;
using Backend.Features.Users;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ValidationException = Backend.Common.Exceptions.ValidationException;

namespace Backend.Tests.Features.Users;

public class UsersServiceTests
{
    private readonly Mock<IUsersRepository> _repositoryMock;
    private readonly Mock<IValidator<CreateUserRequest>> _createValidatorMock;
    private readonly Mock<IValidator<UpdateUserRequest>> _updateValidatorMock;
    private readonly UsersService _sut;

    public UsersServiceTests()
    {
        _repositoryMock = new Mock<IUsersRepository>();
        _createValidatorMock = new Mock<IValidator<CreateUserRequest>>();
        _updateValidatorMock = new Mock<IValidator<UpdateUserRequest>>();

        _sut = new UsersService(
            _repositoryMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object,
            NullLogger<UsersService>.Instance);
    }

    // --- GetAllAsync ---

    [Fact]
    public async Task GetAllAsync_ValidParams_ReturnsPagedResult()
    {
        var entities = new List<User>
        {
            new() { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com", PasswordHash = "hash", CreatedAt = DateTime.UtcNow },
            new() { Id = 2, FirstName = "Jane", LastName = "Doe", Email = "jane@example.com", PasswordHash = "hash", CreatedAt = DateTime.UtcNow }
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
        var entity = new User
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow
        };
        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);

        var result = await _sut.GetByIdAsync(1);

        result.Id.Should().Be(1);
        result.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ThrowsNotFoundException()
    {
        _repositoryMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = () => _sut.GetByIdAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedDto()
    {
        var request = new CreateUserRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Password = "Password1"
        };
        SetupValidValidator(_createValidatorMock, request);
        _repositoryMock
            .Setup(r => r.EmailExistsAsync(request.Email, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User e, CancellationToken _) =>
            {
                e.Id = 1;
                return e;
            });

        var result = await _sut.CreateAsync(request);

        result.Id.Should().Be(1);
        result.Email.Should().Be("john@example.com");
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_InvalidRequest_ThrowsValidationException()
    {
        var request = new CreateUserRequest { FirstName = "", LastName = "", Email = "", Password = "" };
        SetupInvalidValidator(_createValidatorMock, request, "First name is required.");

        var act = () => _sut.CreateAsync(request);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain("First name is required.");
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_DuplicateEmail_ThrowsValidationException()
    {
        var request = new CreateUserRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Password = "Password1"
        };
        SetupValidValidator(_createValidatorMock, request);
        _repositoryMock
            .Setup(r => r.EmailExistsAsync(request.Email, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => _sut.CreateAsync(request);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain("A user with this email already exists.");
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_ValidRequest_ReturnsUpdatedDto()
    {
        var request = new UpdateUserRequest
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com"
        };
        var entity = new User
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow
        };

        SetupValidValidator(_updateValidatorMock, request);
        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _repositoryMock
            .Setup(r => r.EmailExistsAsync(request.Email, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.UpdateAsync(1, request);

        result.FirstName.Should().Be("Jane");
        result.Email.Should().Be("jane@example.com");
        _repositoryMock.Verify(r => r.UpdateAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingId_ThrowsNotFoundException()
    {
        var request = new UpdateUserRequest
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com"
        };
        SetupValidValidator(_updateValidatorMock, request);
        _repositoryMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = () => _sut.UpdateAsync(999, request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_InvalidRequest_ThrowsValidationException()
    {
        var request = new UpdateUserRequest { FirstName = "", LastName = "", Email = "" };
        SetupInvalidValidator(_updateValidatorMock, request, "First name is required.");

        var act = () => _sut.UpdateAsync(1, request);

        await act.Should().ThrowAsync<ValidationException>();
        _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_DuplicateEmail_ThrowsValidationException()
    {
        var request = new UpdateUserRequest
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "existing@example.com"
        };
        var entity = new User
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow
        };

        SetupValidValidator(_updateValidatorMock, request);
        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        _repositoryMock
            .Setup(r => r.EmailExistsAsync(request.Email, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => _sut.UpdateAsync(1, request);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().Contain("A user with this email already exists.");
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_ExistingId_DeletesEntity()
    {
        var entity = new User
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            PasswordHash = "hash"
        };
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
            .ReturnsAsync((User?)null);

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
