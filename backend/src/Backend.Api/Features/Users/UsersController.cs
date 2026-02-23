using Backend.Common.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Features.Users;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUsersService _service;

    public UsersController(IUsersService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<PagedResult<UserDto>>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(page, pageSize, cancellationToken);
        return Ok(ApiResponse<PagedResult<UserDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var user = await _service.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<UserDto>.Ok(user));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserDto>>> Create(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = user.Id },
            ApiResponse<UserDto>.Ok(user));
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserDto>>> Update(int id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _service.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<UserDto>.Ok(user));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<NoContentResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        await _service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
