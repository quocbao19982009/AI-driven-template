using Backend.Common.Models;

namespace Backend.Features.Rooms;

public interface IRoomsService
{
    Task<PagedResult<RoomDto>> GetAllAsync(RoomsListQuery query, CancellationToken cancellationToken = default);
    Task<List<RoomDto>> GetAllUnpagedAsync(CancellationToken cancellationToken = default);
    Task<RoomDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<RoomDto> CreateAsync(CreateRoomRequest request, IFormFile? image, CancellationToken cancellationToken = default);
    Task<RoomDto> UpdateAsync(int id, UpdateRoomRequest request, IFormFile? image, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
