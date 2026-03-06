namespace Backend.Features.Rooms;

public interface IFileStorageService
{
    /// <summary>Saves an uploaded file and returns the relative web path.</summary>
    Task<string> SaveRoomImageAsync(IFormFile file, CancellationToken cancellationToken = default);

    /// <summary>Deletes a file by its relative web path. No-op if path is null.</summary>
    void DeleteRoomImage(string? relativePath);
}
