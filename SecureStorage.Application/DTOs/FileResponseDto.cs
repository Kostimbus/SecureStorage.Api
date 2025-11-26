using System;

namespace SecureStorage.Application.DTOs
{
    public class FileResponseDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long PlaintextSize { get; set; }
        public long EncryptedSize { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
