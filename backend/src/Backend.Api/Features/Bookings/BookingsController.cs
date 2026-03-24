using Backend.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Features.Bookings;

[ApiController]
[Route("api/bookings")]
[AllowAnonymous]
public class BookingsController : ControllerBase
{
    private readonly IBookingsService _bookingsService;

    public BookingsController(IBookingsService bookingsService)
    {
        _bookingsService = bookingsService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<BookingDto>>>> GetAll(
        [FromQuery] GetBookingsQuery query, CancellationToken cancellationToken = default)
    {
        var result = await _bookingsService.GetAllAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<BookingDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<BookingDto>>> GetById(
        int id, CancellationToken cancellationToken = default)
    {
        var item = await _bookingsService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<BookingDto>.Ok(item));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<BookingDto>>> Create(
        CreateBookingRequest request, CancellationToken cancellationToken = default)
    {
        var item = await _bookingsService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id },
            ApiResponse<BookingDto>.Ok(item));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<BookingDto>>> Update(
        int id, UpdateBookingRequest request, CancellationToken cancellationToken = default)
    {
        var item = await _bookingsService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<BookingDto>.Ok(item));
    }

    [HttpDelete("{id}")]
    public async Task<NoContentResult> Delete(
        int id, CancellationToken cancellationToken = default)
    {
        await _bookingsService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
