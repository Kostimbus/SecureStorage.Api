using Microsoft.Extensions.Options;
using SecureStorage.Core.Interfaces;
using SecureStorage.Infrastructure.Options;

public sealed class LocalFileStorage : IFileStorage
{
    private readonly FileStorageOptions _opts;

    public LocalFileStorage(IOptions<FileStorageOptions> opts)
    {
        _opts = opts.Value;
        Directory.CreateDirectory(_opts.BasePath);
    }

    public async Task<string> WriteAsync(Guid fileId, byte[] data, CancellationToken ct)
    {
        var path = Path.Combine(_opts.BasePath, $"{fileId:N}.bin");
        await File.WriteAllBytesAsync(path, data, ct);
        return path;
    }

    public async Task<byte[]> ReadAsync(Guid fileId, CancellationToken ct)
    {
        var path = Path.Combine(_opts.BasePath, $"{fileId:N}.bin");
        return await File.ReadAllBytesAsync(path, ct);
    }

    public Task DeleteAsync(Guid fileId, CancellationToken ct)
    {
        var path = Path.Combine(_opts.BasePath, $"{fileId:N}.bin");
        if (File.Exists(path)) File.Delete(path);
        return Task.CompletedTask;
    }
}
