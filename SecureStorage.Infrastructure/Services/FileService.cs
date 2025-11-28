using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SecureStorage.Application.DTOs;
using SecureStorage.Application.Interfaces;
using SecureStorage.Core.Interfaces;
using SecureStorage.Core.Models;
using SecureStorage.Infrastructure.Crypto;
using SecureStorage.Infrastructure.Options;

namespace SecureStorage.Infrastructure.Services
{
    /// <summary>
    /// Uses per-file salt + PBKDF2(masterKey, salt) -> derived key (AES-256).
    /// AES-GCM payload format: nonce(12) | tag(16) | ciphertext.
    /// </summary>
    public class FileService : IFileService, IDisposable
    {
        private readonly IFileRepository _fileRepo;
        private readonly AesGcmFileEncryptionService _aes; // concrete encryption service with EncryptWithKeyAsync/DecryptWithKeyAsync
        private readonly FileStorageOptions _opts;
        private readonly byte[] _masterKey;

        public FileService(
            IFileRepository fileRepo,
            AesGcmFileEncryptionService aes,
            IOptions<FileStorageOptions> fileOptions,
            IOptions<AesGcmOptions> aesOptions)
        {
            _fileRepo = fileRepo ?? throw new ArgumentNullException(nameof(fileRepo));
            _aes = aes ?? throw new ArgumentNullException(nameof(aes));
            _opts = fileOptions?.Value ?? throw new ArgumentNullException(nameof(fileOptions));
            if (aesOptions?.Value?.Base64Key == null) throw new ArgumentNullException(nameof(aesOptions));
            _masterKey = Convert.FromBase64String(aesOptions.Value.Base64Key);
            if (_masterKey.Length != 32) throw new InvalidOperationException("Master key must be 32 bytes (Base64).");

            // Ensure base path exists
            Directory.CreateDirectory(_opts.BasePath);
        }

        /// <summary>
        /// Upload: reads request.Content into memory (ok for <= MaxFileSizeBytes), derives per-file key, AES-GCM encrypts, writes to disk atomically, persists metadata.
        /// </summary>
        public async Task<SecureStorage.Application.DTOs.FileResponseDto> UploadAsync(UploadFileRequest request, string? description, Guid ownerId, CancellationToken ct = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.Content == null) throw new ArgumentNullException(nameof(request.Content));

            // Validate content type if whitelist exists
            if (_opts.AllowedContentTypes != null && _opts.AllowedContentTypes.Length > 0)
            {
                if (Array.IndexOf(_opts.AllowedContentTypes, request.ContentType) < 0)
                    throw new InvalidOperationException($"Content type '{request.ContentType}' not allowed.");
            }

            // Read stream into memory (respect MaxFileSizeBytes)
            byte[] plaintext;
            if (request.Content.CanSeek)
            {
                if (request.Content.Length == 0) throw new ArgumentException("Empty file");
                if (request.Content.Length > _opts.MaxFileSizeBytes) throw new InvalidOperationException("File too large.");

                request.Content.Position = 0;
                using var ms = new MemoryStream((int)request.Content.Length);
                await request.Content.CopyToAsync(ms, ct);
                plaintext = ms.ToArray();
            }
            else
            {
                using var ms = new MemoryStream();
                await request.Content.CopyToAsync(ms, ct);
                plaintext = ms.ToArray();
                if (plaintext.LongLength == 0) throw new ArgumentException("Empty file");
                if (plaintext.LongLength > _opts.MaxFileSizeBytes) throw new InvalidOperationException("File too large.");
            }

            // Derive key
            var salt = new byte[_opts.SaltSize];
            RandomNumberGenerator.Fill(salt);
            var derivedKey = DeriveKey(_masterKey, salt, _opts.Pbkdf2Iterations);

            byte[] encrypted = Array.Empty<byte>();
            var fileId = Guid.NewGuid();
            var tempPath = Path.Combine(_opts.BasePath, $"{fileId:N}.bin.tmp");
            var finalPath = Path.Combine(_opts.BasePath, $"{fileId:N}.bin");

            try
            {
                // Encrypt with derived key
                encrypted = await _aes.EncryptWithKeyAsync(derivedKey, plaintext, null, ct);

                // Atomic write: write temp file then move
                await File.WriteAllBytesAsync(tempPath, encrypted, ct);
                if (File.Exists(finalPath)) File.Delete(finalPath);
                File.Move(tempPath, finalPath);

                // Persist metadata
                var record = new FileRecord
                {
                    Id = fileId,
                    FileName = request.FileName,
                    ContentType = request.ContentType ?? "application/octet-stream",
                    OwnerId = ownerId,
                    FilePath = finalPath,
                    Salt = salt,
                    PlaintextSize = plaintext.LongLength,
                    EncryptedSize = encrypted.LongLength,
                    CreatedAtUtc = DateTime.UtcNow,
                    Description = request.Description
                };

                await _fileRepo.CreateAsync(record, ct);

                return new SecureStorage.Application.DTOs.FileResponseDto
                {
                    Id = record.Id,
                    FileName = record.FileName,
                    ContentType = record.ContentType,
                    PlaintextSize = record.PlaintextSize,
                    EncryptedSize = record.EncryptedSize,
                    CreatedAtUtc = record.CreatedAtUtc
                };
            }
            finally
            {
                // Clear sensitive buffers
                if (plaintext != null) Array.Clear(plaintext, 0, plaintext.Length);
                if (derivedKey != null) Array.Clear(derivedKey, 0, derivedKey.Length);
                if (encrypted != null) Array.Clear(encrypted, 0, encrypted.Length);

                // cleanup temp file if exists
                try { if (File.Exists(tempPath)) File.Delete(tempPath); } catch { /* swallow */ }
            }
        }

        /// <summary>
        /// Download: read encrypted file from disk, derive key from stored salt, decrypt and return (record, plaintext bytes).
        /// Note: returns plaintext as byte[]; if file sizes can be large, consider streaming variant that returns Stream.
        /// </summary>
        public async Task<(FileRecord Record, byte[] Plaintext)> DownloadAsync(Guid id, Guid requestorId, CancellationToken ct = default)
        {
            var record = await _fileRepo.GetByIdAsync(id, ct);
            if (record == null) throw new FileNotFoundException("File record not found.");
            if (record.OwnerId != requestorId) throw new UnauthorizedAccessException("Not owner of the file.");

            if (!File.Exists(record.FilePath)) throw new FileNotFoundException("Encrypted file not found on disk.");

            // read encrypted payload
            var encrypted = await File.ReadAllBytesAsync(record.FilePath, ct);

            // derive key
            var derivedKey = DeriveKey(_masterKey, record.Salt ?? Array.Empty<byte>(), _opts.Pbkdf2Iterations);

            try
            {
                var plaintext = await _aes.DecryptWithKeyAsync(derivedKey, encrypted, null, ct);
                return (record, plaintext);
            }
            finally
            {
                // clear derivedKey (plaintext returned to caller to handle securely)
                if (derivedKey != null) Array.Clear(derivedKey, 0, derivedKey.Length);
                // encrypted cleared by GC eventually; could zero it now:
                if (encrypted != null) Array.Clear(encrypted, 0, encrypted.Length);
            }
        }

        /// <summary>
        /// Delete: check owner, delete file on disk and metadata in DB.
        /// </summary>
        public async Task<bool> DeleteAsync(Guid id, Guid requestorId, CancellationToken ct = default)
        {
            var record = await _fileRepo.GetByIdAsync(id, ct);
            if (record == null) throw new FileNotFoundException("File record not found.");
            if (record.OwnerId != requestorId) throw new UnauthorizedAccessException("Not owner of the file.");

            bool result;
            try
            {
                if (File.Exists(record.FilePath))
                {
                    File.Delete(record.FilePath);
                }
            }
            finally
            {
                result = await _fileRepo.DeleteAsync(record, ct);
            }
            return result;
        }

        /// <summary>
        /// Optional: list files by owner (simple helper).
        /// </summary>
        public async Task<IEnumerable<FileRecord>> ListByOwnerAsync(Guid ownerId, int page = 1, int pageSize = 50, CancellationToken ct = default)
        {
            return await _fileRepo.ListByOwnerAsync(ownerId, page, pageSize, ct);
        }

        private static byte[] DeriveKey(byte[] masterKey, byte[] salt, int iterations)
        {
            using var kdf = new Rfc2898DeriveBytes(masterKey, salt, iterations, HashAlgorithmName.SHA256);
            return kdf.GetBytes(32);
        }

        public void Dispose()
        {
            if (_masterKey != null) Array.Clear(_masterKey, 0, _masterKey.Length);
            GC.SuppressFinalize(this);
        }
    }
}
