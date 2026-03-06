using Backend.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Features.Bookings;

[ApiController]
[Route("api/bookings")]
public class BookingsController : ControllerBase
{
    private readonly IBookingsService _service;

    public BookingsController(IBookingsService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<BookingDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? roomId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new BookingsListQuery
        {
            Page = page,
            PageSize = pageSize,
            RoomId = roomId,
            FromDate = fromDate,
            ToDate = toDate,
        };
        var result = await _service.GetAllAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<BookingDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<BookingDto>>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var item = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<BookingDto>.Ok(item));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<BookingDto>>> Create(CreateBookingRequest request, CancellationToken cancellationToken = default)
    {
        var item = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, ApiResponse<BookingDto>.Ok(item));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<BookingDto>>> Update(int id, UpdateBookingRequest request, CancellationToken cancellationToken = default)
    {
        var item = await _service.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<BookingDto>.Ok(item));
    }

    [HttpDelete("{id}")]
    public async Task<NoContentResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
