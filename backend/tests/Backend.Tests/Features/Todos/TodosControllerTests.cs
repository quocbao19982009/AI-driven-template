using Backend.Common.Models;
using Backend.Features.Todos;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Backend.Tests.Features.Todos;

public class TodosControllerTests
{
    private readonly Mock<ITodosService> _serviceMock;
    private readonly TodosController _sut;

    public TodosControllerTests()
    {
        _serviceMock = new Mock<ITodosService>();
        _sut = new TodosController(_serviceMock.Object);
    }

    private static TodoDto MakeDto(int id = 1, bool isCompleted = false) =>
        new(id, "Todo Title", null, isCompleted, null, 1, DateTime.UtcNow, null);

    // --- GetAll ---

    [Fact]
    public async Task GetAll_ReturnsOkWithPagedResult()
    {
        var paged = new PagedResult<TodoDto>([MakeDto()], 1, 1, 20);
        _serviceMock
            .Setup(s => s.GetAllAsync(1, 20, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        var result = await _sut.GetAll(1, 20, null, null, null);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<PagedResult<TodoDto>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Items.Should().HaveCount(1);
    }

    // --- GetById ---

    [Fact]
    public async Task GetById_ReturnsOkWithDto()
    {
        _serviceMock
            .Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeDto());

        var result = await _sut.GetById(1);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<TodoDto>>().Subject;
        response.Data!.Id.Should().Be(1);
    }

    // --- Create ---

    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        var request = new CreateTodoRequest { Title = "New Todo", Priority = 1 };
        _serviceMock
            .Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeDto());

        var result = await _sut.Create(request);

        var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(TodosController.GetById));
        created.RouteValues!["id"].Should().Be(1);
        var response = created.Value.Should().BeOfType<ApiResponse<TodoDto>>().Subject;
        response.Data!.Title.Should().Be("Todo Title");
    }

    // --- Update ---

    [Fact]
    public async Task Update_ReturnsOkWithUpdatedDto()
    {
        var request = new UpdateTodoRequest { Title = "Updated", IsCompleted = true, Priority = 2 };
        _serviceMock
            .Setup(s => s.UpdateAsync(1, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeDto(isCompleted: true));

        var result = await _sut.Update(1, request);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<TodoDto>>().Subject;
        response.Data!.IsCompleted.Should().BeTrue();
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

    // --- Toggle ---

    [Fact]
    public async Task Toggle_ReturnsOkWithToggledDto()
    {
        _serviceMock
            .Setup(s => s.ToggleAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeDto(isCompleted: true));

        var result = await _sut.Toggle(1);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<TodoDto>>().Subject;
        response.Data!.IsCompleted.Should().BeTrue();
        _serviceMock.Verify(s => s.ToggleAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }
}
