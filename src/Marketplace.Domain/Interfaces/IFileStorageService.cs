
namespace Marketplace.Domain.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default);
    Task DeleteAsync(string fileUrl, CancellationToken ct = default);
    Task<Stream> DownloadAsync(string fileUrl, CancellationToken ct = default);
}
