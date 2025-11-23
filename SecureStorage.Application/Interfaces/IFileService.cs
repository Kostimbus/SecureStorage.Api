using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SecureStorage.Core.Models;

namespace SecureStorage.Application.Interfaces
{
    public interface IFileService
    {
        Task<Guid> SaveEncryptedAsync(byte[] plaintext, string fileName, string contentType, Guid ownerId, CancellationToken ct = default);
        Task<(FileRecord? fileRecord, byte[]? plaintext)> GetDecryptedAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<FileRecord>> ListByOwnerAsync(Guid ownerId, int page = 1, int pageSize = 50, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
