using Backend.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Features.Rooms;

[ApiController]
[Route("api/rooms")]
public class RoomsController : ControllerBase
{
    private readonly IRoomsService _service;

    public RoomsController(IRoomsService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<RoomDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] int? locationId = null,
        [FromQuery] string sortBy = "createdAt",
        [FromQuery] string sortDir = "asc",
        CancellationToken cancellationToken = default)
    {
        var query = new RoomsListQuery
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            LocationId = locationId,
            SortBy = sortBy,
            SortDir = sortDir,
        };
        var result = await _service.GetAllAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<RoomDto>>.Ok(result));
    }

    [HttpGet("all")]
    public async Task<ActionResult<ApiResponse<List<RoomDto>>>> GetAllUnpaged(CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllUnpagedAsync(cancellationToken);
        return Ok(ApiResponse<List<RoomDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<RoomDto>>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var item = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<RoomDto>.Ok(item));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<RoomDto>>> Create(
        [FromForm] CreateRoomRequest request,
        IFormFile? image,
        CancellationToken cancellationToken = default)
    {
        var item = await _service.CreateAsync(request, image, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, ApiResponse<RoomDto>.Ok(item));
    }

    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<RoomDto>>> Update(
        int id,
        [FromForm] UpdateRoomRequest request,
        IFormFile? image,
        CancellationToken cancellationToken = default)
    {
        var item = await _service.UpdateAsync(id, request, image, cancellationToken);
        return Ok(ApiResponse<RoomDto>.Ok(item));
    }

    [HttpDelete("{id}")]
    public async Task<NoContentResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
