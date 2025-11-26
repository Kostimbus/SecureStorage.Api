using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SecureStorage.Core.Models;
using SecureStorage.Application.DTOs;

namespace SecureStorage.Application.Interfaces
{
    public interface IFileService
    {
        Task<FileResponseDto> UploadAsync(UploadFileRequest request, string? description, Guid ownerId, CancellationToken ct = default);
        Task<(FileRecord Record, byte[] Plaintext)> DownloadAsync(Guid id, Guid requestorId, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, Guid requestorId, CancellationToken ct = default);
        Task<IEnumerable<FileRecord>> ListByOwnerAsync(Guid ownerId, int page = 1, int pageSize = 50, CancellationToken ct = default);
    }
}
