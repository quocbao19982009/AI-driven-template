using Backend.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Features.Factories;

[ApiController]
[Route("api/factories")]
public class FactoriesController : ControllerBase
{
    private readonly IFactoriesService _factoriesService;

    public FactoriesController(IFactoriesService factoriesService)
    {
        _factoriesService = factoriesService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<FactoryDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _factoriesService.GetAllAsync(page, pageSize, search, cancellationToken);
        return Ok(ApiResponse<PagedResult<FactoryDto>>.Ok(result));
    }

    [HttpGet("all")]
    public async Task<ActionResult<ApiResponse<List<FactoryDto>>>> GetAllUnpaged(
        CancellationToken cancellationToken = default)
    {
        var result = await _factoriesService.GetAllUnpagedAsync(cancellationToken);
        return Ok(ApiResponse<List<FactoryDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<FactoryDto>>> GetById(
        int id, CancellationToken cancellationToken = default)
    {
        var item = await _factoriesService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<FactoryDto>.Ok(item));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<FactoryDto>>> Create(
        CreateFactoryRequest request, CancellationToken cancellationToken = default)
    {
        var item = await _factoriesService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id },
            ApiResponse<FactoryDto>.Ok(item));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<FactoryDto>>> Update(
        int id, UpdateFactoryRequest request, CancellationToken cancellationToken = default)
    {
        var item = await _factoriesService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<FactoryDto>.Ok(item));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        int id, CancellationToken cancellationToken = default)
    {
        await _factoriesService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
