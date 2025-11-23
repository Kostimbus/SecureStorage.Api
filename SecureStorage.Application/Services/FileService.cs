using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SecureStorage.Application.Interfaces;
using SecureStorage.Core.Interfaces;
using SecureStorage.Core.Models;

namespace SecureStorage.Application.Services
{
    public class FileService : IFileService
    {
        private readonly IFileRepository _fileRepo;
        private readonly IFileEncryptionService _encryption;

        public FileService(IFileRepository fileRepo, IFileEncryptionService encryption)
        {
            _fileRepo = fileRepo ?? throw new ArgumentNullException(nameof(fileRepo));
            _encryption = encryption ?? throw new ArgumentNullException(nameof(encryption));
        }

        public async Task<Guid> SaveEncryptedAsync(byte[] plaintext, string fileName, string contentType, Guid ownerId, CancellationToken ct = default)
        {
            if (plaintext == null) throw new ArgumentNullException(nameof(plaintext));
            var encrypted = await _encryption.EncryptAsync(plaintext, null, ct).ConfigureAwait(false);

            var record = new FileRecord
            {
                Id = Guid.NewGuid(),
                FileName = fileName ?? string.Empty,
                ContentType = contentType ?? "application/octet-stream",
                OwnerId = ownerId,
                EncryptedData = encrypted,
                PlaintextSize = plaintext.LongLength,
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = null
            };

            return await _fileRepo.CreateAsync(record, ct).ConfigureAwait(false);
        }

        public async Task<(FileRecord? fileRecord, byte[]? plaintext)> GetDecryptedAsync(Guid id, CancellationToken ct = default)
        {
            var record = await _fileRepo.GetByIdAsync(id, ct).ConfigureAwait(false);
            if (record == null)
            {
                return (null, null);
            }
            var plaintext = await _encryption.DecryptAsync(record.EncryptedData, null, ct).ConfigureAwait(false);
            return (record, plaintext);
        }

        public Task<IEnumerable<FileRecord>> ListByOwnerAsync(Guid ownerId, int page = 1, int pageSize = 50, CancellationToken ct = default)
            => _fileRepo.ListByOwnerAsync(ownerId, page, pageSize, ct);

        public Task<bool> DeleteAsync(Guid id, CancellationToken ct = default) 
            => _fileRepo.DeleteAsync(id, ct);
    }
}
