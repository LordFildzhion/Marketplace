
using Microsoft.AspNetCore.Http;

namespace Marketplace.Infrastructure.Storage;

public static class FileValidationHelper
{
    private static readonly HashSet<string> AllowedExtensions = new() { ".jpg", ".jpeg", ".png", ".webp" };
    private const int MaxFileSize = 5 * 1024 * 1024; // 5 MB

    public static void Validate(IFormFile file)
    {
        if (file.Length > MaxFileSize)
            throw new ArgumentException($"File size cannot exceed {MaxFileSize / 1024 / 1024} MB");
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            throw new ArgumentException($"File extension {ext} is not allowed");
    }
}
