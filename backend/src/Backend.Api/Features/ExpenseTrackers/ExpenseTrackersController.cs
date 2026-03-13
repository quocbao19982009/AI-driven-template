using System.Security.Claims;
using Backend.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Features.ExpenseTrackers;

[ApiController]
[Route("api/expense-trackers")]
public class ExpenseTrackersController : ControllerBase
{
    private readonly IExpenseTrackersService _service;

    public ExpenseTrackersController(IExpenseTrackersService service)
    {
        _service = service;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<PagedResult<ExpenseTrackerDto>>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<ExpenseTrackerDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<ExpenseTrackerDto>>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var item = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<ExpenseTrackerDto>.Ok(item));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ExpenseTrackerDto>>> Create(
        CreateExpenseTrackerRequest request, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var item = await _service.CreateAsync(request, userId, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id },
            ApiResponse<ExpenseTrackerDto>.Ok(item));
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ExpenseTrackerDto>>> Update(
        int id, UpdateExpenseTrackerRequest request, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();
        var item = await _service.UpdateAsync(id, request, userId, userRole, cancellationToken);
        return Ok(ApiResponse<ExpenseTrackerDto>.Ok(item));
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<NoContentResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();
        await _service.DeleteAsync(id, userId, userRole, cancellationToken);
        return NoContent();
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!int.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("Invalid token claims.");

        return userId;
    }

    private string GetCurrentUserRole()
    {
        return User.FindFirstValue(ClaimTypes.Role) ?? "User";
    }
}
