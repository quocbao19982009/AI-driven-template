using Backend.Common.Models;
using Backend.Features.Factories;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Backend.Tests.Features.Factories;

public class FactoriesControllerTests
{
    private readonly Mock<IFactoriesService> _serviceMock;
    private readonly FactoriesController _sut;

    public FactoriesControllerTests()
    {
        _serviceMock = new Mock<IFactoriesService>();
        _sut = new FactoriesController(_serviceMock.Object);
    }

    private static FactoryDto MakeDto(int id = 1, string name = "Factory A") =>
        new(id, name, "UTC", DateTime.UtcNow, null);

    // --- GetAll ---

    [Fact]
    public async Task GetAll_ReturnsOkWithPagedResult()
    {
        var paged = new PagedResult<FactoryDto>([MakeDto()], 1, 1, 20);
        _serviceMock
            .Setup(s => s.GetAllAsync(1, 20, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        var result = await _sut.GetAll(1, 20, null);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<PagedResult<FactoryDto>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Items.Should().HaveCount(1);
    }

    // --- GetAllUnpaged ---

    [Fact]
    public async Task GetAllUnpaged_ReturnsOkWithList()
    {
        var list = new List<FactoryDto> { MakeDto(1), MakeDto(2, "Factory B") };
        _serviceMock
            .Setup(s => s.GetAllUnpagedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        var result = await _sut.GetAllUnpaged();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<List<FactoryDto>>>().Subject;
        response.Data!.Should().HaveCount(2);
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
        var response = ok.Value.Should().BeOfType<ApiResponse<FactoryDto>>().Subject;
        response.Data!.Id.Should().Be(1);
        response.Data.Name.Should().Be("Factory A");
    }

    // --- Create ---

    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        var request = new CreateFactoryRequest { Name = "New Factory", TimeZone = "UTC" };
        _serviceMock
            .Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeDto());

        var result = await _sut.Create(request);

        var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(FactoriesController.GetById));
        created.RouteValues!["id"].Should().Be(1);
        var response = created.Value.Should().BeOfType<ApiResponse<FactoryDto>>().Subject;
        response.Data!.Name.Should().Be("Factory A");
    }

    // --- Update ---

    [Fact]
    public async Task Update_ReturnsOkWithUpdatedDto()
    {
        var request = new UpdateFactoryRequest { Name = "Updated Factory", TimeZone = "UTC+1" };
        _serviceMock
            .Setup(s => s.UpdateAsync(1, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeDto(1, "Updated Factory"));

        var result = await _sut.Update(1, request);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<FactoryDto>>().Subject;
        response.Data!.Name.Should().Be("Updated Factory");
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
