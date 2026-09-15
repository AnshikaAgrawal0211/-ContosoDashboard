using System.Security.Cryptography;

namespace ContosoDashboard.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;

    public LocalFileStorageService(IWebHostEnvironment environment)
    {
        var appDataRoot = Path.Combine(environment.ContentRootPath, "AppData", "uploads");
        Directory.CreateDirectory(appDataRoot);
        _rootPath = appDataRoot;
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType)
    {
        var safeFileName = Path.GetFileName(fileName);
        var extension = Path.GetExtension(safeFileName);
        var uniqueName = $"{Guid.NewGuid():N}{extension}";
        var relativePath = Path.Combine(Guid.NewGuid().ToString("N")[..2], uniqueName);
        var fullPath = Path.Combine(_rootPath, relativePath);

        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var output = File.Create(fullPath);
        await fileStream.CopyToAsync(output);

        return relativePath.Replace('\\', '/');
    }

    public Task DeleteAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return Task.CompletedTask;
        }

        var fullPath = Path.Combine(_rootPath, filePath.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    public Task<Stream> DownloadAsync(string filePath)
    {
        var fullPath = Path.Combine(_rootPath, filePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullPath))
        {
            return Task.FromResult<Stream>(new MemoryStream());
        }

        return Task.FromResult<Stream>(File.OpenRead(fullPath));
    }

    public Task<string> GetUrlAsync(string filePath, TimeSpan expiration)
    {
        return Task.FromResult($"/documents/download?path={Uri.EscapeDataString(filePath)}");
    }
}
