using Microsoft.Extensions.Logging;
using SecureStorage.Application.DTOs;
using SecureStorage.Application.Interfaces;
using SecureStorage.Core.Interfaces;
using SecureStorage.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace SecureStorage.Application.Services
{
    /// <summary>
    /// Application-level file service.
    /// Handles validation, authorization, logging and orchestration.
    /// Encryption, storage and persistence are delegated.
    /// </summary>
    public sealed class FileService : IFileService
    {
        private readonly IFileRepository _fileRepo;
        private readonly IFileCryptoService _crypto;
        private readonly IFileStorage _storage;
        private readonly ILogger<FileService> _logger;

        public FileService(
            IFileRepository fileRepo,
            IFileCryptoService crypto,
            IFileStorage storage,
            ILogger<FileService> logger)
        {
            _fileRepo = fileRepo ?? throw new ArgumentNullException(nameof(fileRepo));
            _crypto = crypto ?? throw new ArgumentNullException(nameof(crypto));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Upload: reads request.Content into memory (ok for <= MaxFileSizeBytes), derives per-file key, AES-GCM encrypts, writes to disk atomically, persists metadata.
        /// </summary>
        public async Task<FileResponseDto> UploadAsync(
            UploadFileRequest request,
            string? description,
            Guid ownerId,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentNullException.ThrowIfNull(request.Content);

            _logger.LogInformation(
                "Uploading file {FileName} for OwnerId={OwnerId}",
                request.FileName,
                ownerId);

            byte[] plaintext;
            using (var ms = new MemoryStream())
            {
                await request.Content.CopyToAsync(ms, ct);
                plaintext = ms.ToArray();
            }

            if (plaintext.Length == 0)
                throw new InvalidOperationException("Empty file");

            var salt = RandomNumberGenerator.GetBytes(16);
            var encrypted = await _crypto.EncryptAsync(plaintext, salt, ct);

            Array.Clear(plaintext, 0, plaintext.Length);

            var fileId = Guid.NewGuid();
            var filePath = await _storage.WriteAsync(fileId, encrypted, ct);

            var record = new FileRecord
            {
                Id = fileId,
                FileName = request.FileName,
                ContentType = request.ContentType ?? "application/octet-stream",
                OwnerId = ownerId,
                FilePath = filePath,
                Salt = salt,
                PlaintextSize = plaintext.Length,
                EncryptedSize = encrypted.Length,
                CreatedAtUtc = DateTime.UtcNow,
                Description = description
            };

            await _fileRepo.CreateAsync(record, ct);

            _logger.LogInformation(
                "File uploaded successfully. FileId={FileId}, OwnerId={OwnerId}",
                fileId,
                ownerId);

            return new FileResponseDto
            {
                Id = record.Id,
                FileName = record.FileName,
                ContentType = record.ContentType,
                PlaintextSize = record.PlaintextSize,
                EncryptedSize = record.EncryptedSize,
                CreatedAtUtc = record.CreatedAtUtc
            };
        }

        /// <summary>
        /// Download: read encrypted file from disk, derive key from stored salt, decrypt and return (record, plaintext bytes).
        /// Note: returns plaintext as byte[]; if file sizes can be large, consider streaming variant that returns Stream.
        /// </summary>
        public async Task<(FileRecord Record, byte[] Plaintext)> DownloadAsync(
            Guid fileId,
            Guid requestorId,
            CancellationToken ct = default)
        {
            var record = await _fileRepo.GetByIdAsync(fileId, ct)
                ?? throw new FileNotFoundException("File not found.");

            if (record.OwnerId != requestorId)
            {
                _logger.LogWarning(
                    "Download denied. FileId={FileId}, OwnerId={OwnerId}, RequestorId={RequestorId}",
                    fileId, record.OwnerId, requestorId);

                throw new UnauthorizedAccessException("Not owner of the file.");
            }

            var encrypted = await _storage.ReadAsync(fileId, ct);
            var plaintext = await _crypto.DecryptAsync(encrypted, record.Salt!, ct);

            _logger.LogInformation(
                "File downloaded. FileId={FileId}, OwnerId={OwnerId}",
                fileId, requestorId);

            return (record, plaintext);
        }

        /// <summary>
        /// Delete: check owner, delete file on disk and metadata in DB.
        /// </summary>
        public async Task<bool> DeleteAsync(
            Guid fileId,
            Guid requestorId,
            CancellationToken ct = default)
        {
            var record = await _fileRepo.GetByIdAsync(fileId, ct)
                ?? throw new FileNotFoundException("File not found.");

            if (record.OwnerId != requestorId)
            {
                _logger.LogWarning(
                    "Delete denied. FileId={FileId}, OwnerId={OwnerId}, RequestorId={RequestorId}",
                    fileId, record.OwnerId, requestorId);

                throw new UnauthorizedAccessException("Not owner of the file.");
            }

            await _storage.DeleteAsync(fileId, ct);
            var result = await _fileRepo.DeleteAsync(record, ct);

            _logger.LogInformation(
                "File deleted. FileId={FileId}, OwnerId={OwnerId}",
                fileId, requestorId);

            return result;
        }

        /// <summary>
        /// Check if requestorId is owner of fileId.
        /// </summary>
        public async Task<bool> IsOwnerAsync(
            Guid fileId,
            Guid requestorId,
            CancellationToken ct = default)
        {
            _logger.LogDebug(
                "Checking ownership. FileId={FileId}, RequestorId={RequestorId}",
                fileId, requestorId);

            var record = await _fileRepo.GetByIdAsync(fileId, ct);
            if (record == null)
            {
                _logger.LogWarning(
                    "Ownership check failed: file not found. FileId={FileId}",
                    fileId);
                return false;
            }

            return record.OwnerId == requestorId;
        }

        /// <summary>
        /// List files by owner (simple helper).
        /// </summary>
        public Task<IEnumerable<FileRecord>> ListByOwnerAsync(
            Guid ownerId,
            int page = 1,
            int pageSize = 50,
            CancellationToken ct = default)
        {
            return _fileRepo.ListByOwnerAsync(ownerId, page, pageSize, ct);
        }
    }
}