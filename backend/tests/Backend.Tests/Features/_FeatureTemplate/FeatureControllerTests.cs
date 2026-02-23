using Backend.Common.Models;
using Backend.Features._FeatureTemplate;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Backend.Tests.Features._FeatureTemplate;

public class FeatureControllerTests
{
    private readonly Mock<IFeatureService> _serviceMock;
    private readonly FeatureController _sut;

    public FeatureControllerTests()
    {
        _serviceMock = new Mock<IFeatureService>();
        _sut = new FeatureController(_serviceMock.Object);
    }

    // --- GetAll ---

    [Fact]
    public async Task GetAll_ReturnsOkWithPagedResult()
    {
        var items = new List<FeatureDto> { new(1, "Item", DateTime.UtcNow) };
        var paged = new PagedResult<FeatureDto>(items, 1, 1, 20);
        _serviceMock
            .Setup(s => s.GetAllAsync(1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        var result = await _sut.GetAll(1, 20);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<PagedResult<FeatureDto>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Items.Should().HaveCount(1);
    }

    // --- GetById ---

    [Fact]
    public async Task GetById_ReturnsOkWithDto()
    {
        var dto = new FeatureDto(1, "Test", DateTime.UtcNow);
        _serviceMock
            .Setup(s => s.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _sut.GetById(1);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<FeatureDto>>().Subject;
        response.Data!.Name.Should().Be("Test");
    }

    // --- Create ---

    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        var request = new CreateFeatureRequest { Name = "New" };
        var dto = new FeatureDto(1, "New", DateTime.UtcNow);
        _serviceMock
            .Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _sut.Create(request);

        var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(FeatureController.GetById));
        created.RouteValues!["id"].Should().Be(1);
        var response = created.Value.Should().BeOfType<ApiResponse<FeatureDto>>().Subject;
        response.Data!.Name.Should().Be("New");
    }

    // --- Update ---

    [Fact]
    public async Task Update_ReturnsOkWithUpdatedDto()
    {
        var request = new UpdateFeatureRequest { Name = "Updated" };
        var dto = new FeatureDto(1, "Updated", DateTime.UtcNow);
        _serviceMock
            .Setup(s => s.UpdateAsync(1, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        var result = await _sut.Update(1, request);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<FeatureDto>>().Subject;
        response.Data!.Name.Should().Be("Updated");
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
