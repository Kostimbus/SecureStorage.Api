namespace SecureStorage.Api.DTOs
{
    /// <summary>
    /// Response DTO for admin file listing. Never contains encrypted bytes.
    /// </summary>
    public class FileAdminDto
    {
        // File info
        public Guid Id { get; set; }
        public string Filename { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long PlaintextSize { get; set; }
        public long EncryptedSize { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public string? Description { get; set; }

        // Owner info
        public Guid OwnerId { get; set; }
        public string OwnerUsername { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;
        public bool OwnerIsDisabled { get; set; }
    }
}
