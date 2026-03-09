using Backend.Common.Models;
using Backend.Features.Todos;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Backend.Tests.Features.Todos;

public class TodoControllerTests
{
    private readonly Mock<ITodosService> _serviceMock;
    private readonly TodosController _sut;

    public TodoControllerTests()
    {
        _serviceMock = new Mock<ITodosService>();
        _sut = new TodosController(_serviceMock.Object);
    }

    // --- GetAll ---

    [Fact]
    public async Task GetAll_ReturnsOkWithPagedResult()
    {
        var items = new List<TodoDto>
        {
            new(1, "Todo 1", null, false, null, DateTime.UtcNow, null)
        };
        var paged = new PagedResult<TodoDto>(items, 1, 1, 20);
        _serviceMock
            .Setup(s => s.GetAllAsync(1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        var result = await _sut.GetAll(1, 20);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<PagedResult<TodoDto>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Items.Should().HaveCount(1);
    }

    // --- GetById ---

    [Fact]
    public async Task GetById_ReturnsOkWithDto()
    {
        var dto = new TodoDto(1, "Test", null, false, null, DateTime.UtcNow, null);
        _serviceMock
            .Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _sut.GetById(1);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<TodoDto>>().Subject;
        response.Data!.Title.Should().Be("Test");
    }

    // --- Create ---

    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        var request = new CreateTodoRequest { Title = "New todo" };
        var dto = new TodoDto(1, "New todo", null, false, null, DateTime.UtcNow, null);
        _serviceMock
            .Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _sut.Create(request);

        var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(TodosController.GetById));
        created.RouteValues!["id"].Should().Be(1);
        var response = created.Value.Should().BeOfType<ApiResponse<TodoDto>>().Subject;
        response.Data!.Title.Should().Be("New todo");
        response.Data!.IsCompleted.Should().BeFalse();
    }

    // --- Update ---

    [Fact]
    public async Task Update_ReturnsOkWithUpdatedDto()
    {
        var request = new UpdateTodoRequest { Title = "Updated" };
        var dto = new TodoDto(1, "Updated", null, false, null, DateTime.UtcNow, null);
        _serviceMock
            .Setup(s => s.UpdateAsync(1, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _sut.Update(1, request);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<TodoDto>>().Subject;
        response.Data!.Title.Should().Be("Updated");
    }

    // --- Toggle ---

    [Fact]
    public async Task Toggle_ReturnsOkWithToggledDto()
    {
        var dto = new TodoDto(1, "Test", null, true, null, DateTime.UtcNow, null);
        _serviceMock
            .Setup(s => s.ToggleAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _sut.Toggle(1);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<TodoDto>>().Subject;
        response.Data!.IsCompleted.Should().BeTrue();
        _serviceMock.Verify(s => s.ToggleAsync(1, It.IsAny<CancellationToken>()), Times.Once);
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
