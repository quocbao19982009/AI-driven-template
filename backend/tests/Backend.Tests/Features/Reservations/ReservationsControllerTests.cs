using Backend.Common.Models;
using Backend.Features.Reservations;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Backend.Tests.Features.Reservations;

public class ReservationsControllerTests
{
    private readonly Mock<IReservationsService> _serviceMock;
    private readonly ReservationsController _sut;

    private static readonly DateTime Start = new(2026, 6, 1, 8, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime End = new(2026, 6, 1, 16, 0, 0, DateTimeKind.Utc);

    public ReservationsControllerTests()
    {
        _serviceMock = new Mock<IReservationsService>();
        _sut = new ReservationsController(_serviceMock.Object);
    }

    private static ReservationDto MakeDto(int id = 1) =>
        new(id, 1, "Factory A", Start, End, 8.0,
            [new ReservationPersonDto(1, 1, "Alice Smith")],
            DateTime.UtcNow, null);

    // --- GetAll ---

    [Fact]
    public async Task GetAll_ReturnsOkWithPagedResult()
    {
        var paged = new PagedResult<ReservationDto>([MakeDto()], 1, 1, 20);
        _serviceMock
            .Setup(s => s.GetAllAsync(1, 20, null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        var result = await _sut.GetAll(1, 20, null, null, null, null);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<PagedResult<ReservationDto>>>().Subject;
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
        var response = ok.Value.Should().BeOfType<ApiResponse<ReservationDto>>().Subject;
        response.Data!.Id.Should().Be(1);
        response.Data.FactoryDisplayName.Should().Be("Factory A");
        response.Data.DurationHours.Should().Be(8.0);
    }

    // --- Create ---

    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        var request = new CreateReservationRequest
        {
            FactoryId = 1, StartTime = Start, EndTime = End, PersonIds = [1]
        };
        _serviceMock
            .Setup(s => s.CreateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeDto());

        var result = await _sut.Create(request);

        var created = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.ActionName.Should().Be(nameof(ReservationsController.GetById));
        created.RouteValues!["id"].Should().Be(1);
        var response = created.Value.Should().BeOfType<ApiResponse<ReservationDto>>().Subject;
        response.Data!.Personnel.Should().HaveCount(1);
    }

    // --- Update ---

    [Fact]
    public async Task Update_ReturnsOkWithUpdatedDto()
    {
        var request = new UpdateReservationRequest
        {
            FactoryId = 1, StartTime = Start, EndTime = End, PersonIds = [1]
        };
        _serviceMock
            .Setup(s => s.UpdateAsync(1, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(MakeDto());

        var result = await _sut.Update(1, request);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var response = ok.Value.Should().BeOfType<ApiResponse<ReservationDto>>().Subject;
        response.Data!.FactoryDisplayName.Should().Be("Factory A");
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
