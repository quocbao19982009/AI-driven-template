using Backend.Common.Models;
using Backend.Features.Factories;
using Backend.Features.Personnel;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Backend.Tests.Features.Personnel;

public class PersonnelControllerTests
{
    private readonly Mock<IPersonnelService> _serviceMock;
    private readonly PersonnelController _sut;

    public PersonnelControllerTests()
    {
        _serviceMock = new Mock<IPersonnelService>();
        _sut = new PersonnelController(_serviceMock.Object);
    }

    private static PersonDto MakeDto(int id = 1, string fullName = "Alice Smith") =>
        new(id, "P001", fullName, "alice@test.com", [], DateTime.UtcNow, null);

    // --- GetAll ---

    [Fact]
    public async Task GetAll_ReturnsOkWithPagedResult()
    {
        var paged = new PagedResult<PersonDto>([MakeDto()], 1, 1, 20);
        _serviceMock
            .Setup(s => s.GetAllAsync(1, 20, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        var result = await _sut.GetAll(1, 20, null);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<PagedResult<PersonDto>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.Items.Should().HaveCount(1);
    }

    // --- GetAllUnpaged ---

    [Fact]
    public async Task GetAllUnpaged_ReturnsOkWithList()
    {
        var list = new List<PersonDto> { MakeDto(1), MakeDto(2, "Bob Jones") };
        _serviceMock
            .Setup(s => s.GetAllUnpagedAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        var result = await _sut.GetAllUnpaged();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<List<PersonDto>>>().Subject;
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
        var response = ok.Value.Should().BeOfType<ApiResponse<PersonDto>>().Subject;
        response.Data!.Id.Should().Be(1);
        response.Data.FullName.Should().Be("Alice Smith");
    }

    // --- Create ---

    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        var request = new CreatePersonRequest
        {
            PersonalId = "P001", FullName = "Alice Smith",
            Email = "alice@test.com", AllowedFactoryIds = []
        };
        _serviceMock
            .Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeDto());

        var result = await _sut.Create(request);

        var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(PersonnelController.GetById));
        created.RouteValues!["id"].Should().Be(1);
        var response = created.Value.Should().BeOfType<ApiResponse<PersonDto>>().Subject;
        response.Data!.FullName.Should().Be("Alice Smith");
    }

    // --- Update ---

    [Fact]
    public async Task Update_ReturnsOkWithUpdatedDto()
    {
        var request = new UpdatePersonRequest
        {
            PersonalId = "P001", FullName = "Alice Updated",
            Email = "alice.updated@test.com", AllowedFactoryIds = []
        };
        _serviceMock
            .Setup(s => s.UpdateAsync(1, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeDto(1, "Alice Updated"));

        var result = await _sut.Update(1, request);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<PersonDto>>().Subject;
        response.Data!.FullName.Should().Be("Alice Updated");
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
