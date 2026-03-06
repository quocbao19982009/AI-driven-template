using Backend.Common.Exceptions;

namespace Backend.Features.Rooms;

public class FileStorageService : IFileStorageService
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png"];
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    private readonly IWebHostEnvironment _env;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(IWebHostEnvironment env, ILogger<FileStorageService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<string> SaveRoomImageAsync(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file.Length > MaxFileSizeBytes)
            throw new ValidationException(["Image must not exceed 5 MB."]);

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            throw new ValidationException(["Only JPEG and PNG images are allowed."]);

        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploadDir = Path.Combine(webRoot, "uploads", "rooms");
        Directory.CreateDirectory(uploadDir);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadDir, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream, cancellationToken);

        _logger.LogInformation("Saved room image {FileName}", fileName);
        return $"/uploads/rooms/{fileName}";
    }

    public void DeleteRoomImage(string? relativePath)
    {
        if (string.IsNullOrEmpty(relativePath)) return;

        var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var fullPath = Path.Combine(webRoot, relativePath.TrimStart('/'));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("Deleted room image {Path}", relativePath);
        }
    }
}
