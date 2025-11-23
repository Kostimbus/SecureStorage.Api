using System;

namespace SecureStorage.Core.Models
{
    /// <summary>
    /// Domain model representing an encrypted file stored in the system.
    /// The EncryptedData property stores the encrypted bytes produced by the encryption service.
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
        /// The owner user's Id.
        /// </summary>
        public Guid OwnerId { get; init; }

        /// <summary>
        /// Encrypted payload bytes. Implementation-specific format.
        /// </summary>
        public byte[] EncryptedData { get; init; } = Array.Empty<byte>();

        /// <summary>
        /// Size of original plaintext in bytes.
        /// </summary>
        public long PlaintextSize { get; init; }

        /// <summary>
        /// Size of stored encrypted payload in bytes.
        /// </summary>
        public long EncryptedSize => EncryptedData?.LongLength ?? 0;

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
