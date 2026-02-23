using Backend.Common.Models;

namespace Backend.Features.Users;

public interface IUsersService
{
    Task<PagedResult<UserDto>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<UserDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserDto> UpdateAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
