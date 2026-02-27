using Backend.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Features.Reservations;

[ApiController]
[Route("api/reservations")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationsService _reservationsService;

    public ReservationsController(IReservationsService reservationsService)
    {
        _reservationsService = reservationsService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ReservationDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? factoryId = null,
        [FromQuery] int? personId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _reservationsService.GetAllAsync(
            page, pageSize, factoryId, personId, fromDate, toDate, cancellationToken);
        return Ok(ApiResponse<PagedResult<ReservationDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ReservationDto>>> GetById(
        int id, CancellationToken cancellationToken = default)
    {
        var item = await _reservationsService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<ReservationDto>.Ok(item));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ReservationDto>>> Create(
        CreateReservationRequest request, CancellationToken cancellationToken = default)
    {
        var item = await _reservationsService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id },
            ApiResponse<ReservationDto>.Ok(item));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ReservationDto>>> Update(
        int id, UpdateReservationRequest request, CancellationToken cancellationToken = default)
    {
        var item = await _reservationsService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<ReservationDto>.Ok(item));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        int id, CancellationToken cancellationToken = default)
    {
        await _reservationsService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
