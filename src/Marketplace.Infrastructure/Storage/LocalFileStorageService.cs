using Marketplace.Domain.Interfaces;
using Microsoft.Extensions.Hosting;

namespace Marketplace.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IHostEnvironment _env;
    public LocalFileStorageService(IHostEnvironment env) => _env = env;

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default)
    {
        var uploadsPath = Path.Combine(_env.ContentRootPath, "wwwroot", "uploads");
        Directory.CreateDirectory(uploadsPath);
        var uniqueName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(uploadsPath, uniqueName);
        using var fileWriter = File.Create(filePath);
        await fileStream.CopyToAsync(fileWriter, ct);
        return $"/uploads/{uniqueName}";
    }

    public Task DeleteAsync(string fileUrl, CancellationToken ct = default)
    {
        var path = Path.Combine(_env.ContentRootPath, "wwwroot", fileUrl.TrimStart('/'));
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }

    public Task<Stream> DownloadAsync(string fileUrl, CancellationToken ct = default)
    {
        var path = Path.Combine(_env.ContentRootPath, "wwwroot", fileUrl.TrimStart('/'));
        if (!File.Exists(path)) throw new FileNotFoundException("File not found", path);
        return Task.FromResult<Stream>(File.OpenRead(path));
    }
}
