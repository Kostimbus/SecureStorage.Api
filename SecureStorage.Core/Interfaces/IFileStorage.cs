namespace SecureStorage.Core.Interfaces
{
    public interface IFileStorage
    {
        Task<string> WriteAsync(Guid fileId, byte[] data, CancellationToken ct = default);
        Task<byte[]> ReadAsync(Guid fileId, CancellationToken ct = default);
        Task DeleteAsync(Guid fileId, CancellationToken ct = default);
    }
}
