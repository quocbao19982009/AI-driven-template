using Backend.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Features.Rooms;

[ApiController]
[Route("api/rooms")]
[AllowAnonymous]
public class RoomsController : ControllerBase
{
    private readonly IRoomsService _roomsService;

    public RoomsController(IRoomsService roomsService)
    {
        _roomsService = roomsService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<RoomDto>>>> GetAll(
        [FromQuery] GetRoomsQuery query, CancellationToken cancellationToken = default)
    {
        var result = await _roomsService.GetAllAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<RoomDto>>.Ok(result));
    }

    [HttpGet("all")]
    public async Task<ActionResult<ApiResponse<List<RoomDto>>>> GetAllUnpaged(
        CancellationToken cancellationToken = default)
    {
        var result = await _roomsService.GetAllUnpagedAsync(cancellationToken);
        return Ok(ApiResponse<List<RoomDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<RoomDto>>> GetById(
        int id, CancellationToken cancellationToken = default)
    {
        var item = await _roomsService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<RoomDto>.Ok(item));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<RoomDto>>> Create(
        [FromForm] CreateRoomRequest request, CancellationToken cancellationToken = default)
    {
        var item = await _roomsService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id },
            ApiResponse<RoomDto>.Ok(item));
    }

    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<RoomDto>>> Update(
        int id, [FromForm] UpdateRoomRequest request, CancellationToken cancellationToken = default)
    {
        var item = await _roomsService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<RoomDto>.Ok(item));
    }

    [HttpDelete("{id}")]
    public async Task<NoContentResult> Delete(
        int id, CancellationToken cancellationToken = default)
    {
        await _roomsService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
