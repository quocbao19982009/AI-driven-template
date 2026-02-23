using Backend.Common.Models;
using Backend.Features.Users;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Backend.Tests.Features.Users;

public class UsersControllerTests
{
    private readonly Mock<IUsersService> _serviceMock;
    private readonly UsersController _sut;

    public UsersControllerTests()
    {
        _serviceMock = new Mock<IUsersService>();
        _sut = new UsersController(_serviceMock.Object);
    }

    // --- GetAll ---

    [Fact]
    public async Task GetAll_ReturnsOkWithPagedResult()
    {
        var items = new List<UserDto> { new(1, "John", "Doe", "john@example.com", "User", true, DateTime.UtcNow) };
        var paged = new PagedResult<UserDto>(items, 1, 1, 20);
        _serviceMock
            .Setup(s => s.GetAllAsync(1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        var result = await _sut.GetAll(1, 20);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<PagedResult<UserDto>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Items.Should().HaveCount(1);
    }

    // --- GetById ---

    [Fact]
    public async Task GetById_ReturnsOkWithDto()
    {
        var dto = new UserDto(1, "John", "Doe", "john@example.com", "User", true, DateTime.UtcNow);
        _serviceMock
            .Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _sut.GetById(1);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<UserDto>>().Subject;
        response.Data!.Email.Should().Be("john@example.com");
    }

    // --- Create ---

    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        var request = new CreateUserRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Password = "Password1"
        };
        var dto = new UserDto(1, "John", "Doe", "john@example.com", "User", true, DateTime.UtcNow);
        _serviceMock
            .Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _sut.Create(request);

        var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(UsersController.GetById));
        created.RouteValues!["id"].Should().Be(1);
        var response = created.Value.Should().BeOfType<ApiResponse<UserDto>>().Subject;
        response.Data!.Email.Should().Be("john@example.com");
    }

    // --- Update ---

    [Fact]
    public async Task Update_ReturnsOkWithUpdatedDto()
    {
        var request = new UpdateUserRequest
        {
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@example.com"
        };
        var dto = new UserDto(1, "Jane", "Doe", "jane@example.com", "User", true, DateTime.UtcNow);
        _serviceMock
            .Setup(s => s.UpdateAsync(1, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _sut.Update(1, request);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<UserDto>>().Subject;
        response.Data!.FirstName.Should().Be("Jane");
    }

    // --- Delete ---

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        _serviceMock
            .Setup(s => s.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.Delete(1);

        result.Should().BeOfType<NoContentResult>();
        _serviceMock.Verify(s => s.DeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }
}
