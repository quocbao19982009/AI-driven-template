using Backend.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Features._FeatureTemplate;

// TODO: Rename to match your entity (e.g., ProductsController)
// TODO: Update route to match your entity (e.g., "api/products")
// TODO: Adjust [Authorize] per endpoint — use [AllowAnonymous] for public endpoints,
//       [Authorize(Roles = "Admin")] for admin-only, or [Authorize] for any authenticated user.

[ApiController]
[Route("api/features")]
[Authorize]
public class FeatureController : ControllerBase
{
    private readonly IFeatureService _featureService;

    public FeatureController(IFeatureService featureService)
    {
        _featureService = featureService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<FeatureDto>>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _featureService.GetAllAsync(page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<FeatureDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<FeatureDto>>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var item = await _featureService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<FeatureDto>.Ok(item));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<FeatureDto>>> Create(CreateFeatureRequest request, CancellationToken cancellationToken = default)
    {
        var item = await _featureService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id },
            ApiResponse<FeatureDto>.Ok(item));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<FeatureDto>>> Update(int id, UpdateFeatureRequest request, CancellationToken cancellationToken = default)
    {
        var item = await _featureService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<FeatureDto>.Ok(item));
    }

    [HttpDelete("{id}")]
    public async Task<NoContentResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        await _featureService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
