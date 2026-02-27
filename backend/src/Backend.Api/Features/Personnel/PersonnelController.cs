using Backend.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Features.Personnel;

[ApiController]
[Route("api/personnel")]
public class PersonnelController : ControllerBase
{
    private readonly IPersonnelService _personnelService;

    public PersonnelController(IPersonnelService personnelService)
    {
        _personnelService = personnelService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<PersonDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _personnelService.GetAllAsync(page, pageSize, search, cancellationToken);
        return Ok(ApiResponse<PagedResult<PersonDto>>.Ok(result));
    }

    [HttpGet("all")]
    public async Task<ActionResult<ApiResponse<List<PersonDto>>>> GetAllUnpaged(
        CancellationToken cancellationToken = default)
    {
        var result = await _personnelService.GetAllUnpagedAsync(cancellationToken);
        return Ok(ApiResponse<List<PersonDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<PersonDto>>> GetById(
        int id, CancellationToken cancellationToken = default)
    {
        var item = await _personnelService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<PersonDto>.Ok(item));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PersonDto>>> Create(
        CreatePersonRequest request, CancellationToken cancellationToken = default)
    {
        var item = await _personnelService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id },
            ApiResponse<PersonDto>.Ok(item));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<PersonDto>>> Update(
        int id, UpdatePersonRequest request, CancellationToken cancellationToken = default)
    {
        var item = await _personnelService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<PersonDto>.Ok(item));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        int id, CancellationToken cancellationToken = default)
    {
        await _personnelService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
