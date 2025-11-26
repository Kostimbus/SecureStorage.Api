using System;

namespace SecureStorage.Core.Models
{
    /// <summary>
    /// Encrypted file metadata. Ciphertext is stored on disk (FilePath).
    /// Per-file salt (for PBKDF2 key derivation) is stored as byte[].
    /// </summary>
    public class FileRecord
    {
        /// <summary>
        /// Primary id (GUID).
        /// </summary>
        public Guid Id { get; init; } = Guid.NewGuid();

        /// <summary>
        /// File name as uploaded by the user (e.g. "report.pdf").
        /// </summary>
        public string FileName { get; init; } = string.Empty;

        /// <summary>
        /// MIME content type (e.g. "application/pdf").
        /// </summary>
        public string ContentType { get; init; } = "application/octet-stream";

        /// <summary>
        /// Path (relative or absolute) to stored encrypted file (nonce|tag|ciphertext).
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// The owner user's Id.
        /// </summary>
        public Guid OwnerId { get; init; }

        /// <summary>
        /// Per-file salt used to derive per-file key from master key (Rfc2898).
        /// Stored as blob/byte[] in DB.
        /// </summary>
        public byte[] Salt { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Size of original plaintext in bytes.
        /// </summary>
        public long PlaintextSize { get; init; }

        /// <summary>
        /// Ciphertext size in bytes (stored file length).
        /// </summary>
        public long EncryptedSize { get; set; }

        /// <summary>
        /// When the record was created (UTC).
        /// </summary>
        public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

        /// <summary>
        /// When the record was last updated (UTC).
        /// </summary>
        public DateTime? UpdatedAtUtc { get; set; }

        /// <summary>
        /// Optional free-text description or tags; keep length reasonable.
        /// </summary>
        public string? Description { get; init; }
    }
}
