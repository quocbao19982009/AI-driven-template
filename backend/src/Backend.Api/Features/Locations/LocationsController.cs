using Backend.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Features.Locations;

[ApiController]
[Route("api/locations")]
[AllowAnonymous]
public class LocationsController : ControllerBase
{
    private readonly ILocationsService _locationsService;

    public LocationsController(ILocationsService locationsService)
    {
        _locationsService = locationsService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<LocationDto>>>> GetAll(
        CancellationToken cancellationToken = default)
    {
        var result = await _locationsService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<List<LocationDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<LocationDto>>> GetById(
        int id, CancellationToken cancellationToken = default)
    {
        var item = await _locationsService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<LocationDto>.Ok(item));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<LocationDto>>> Create(
        CreateLocationRequest request, CancellationToken cancellationToken = default)
    {
        var item = await _locationsService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id },
            ApiResponse<LocationDto>.Ok(item));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<LocationDto>>> Update(
        int id, UpdateLocationRequest request, CancellationToken cancellationToken = default)
    {
        var item = await _locationsService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<LocationDto>.Ok(item));
    }

    [HttpDelete("{id}")]
    public async Task<NoContentResult> Delete(
        int id, CancellationToken cancellationToken = default)
    {
        await _locationsService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
