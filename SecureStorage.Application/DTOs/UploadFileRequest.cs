using System.IO;

namespace SecureStorage.Application.DTOs
{
    public class UploadFileRequest
    {
        public Stream Content { get; set; } = default!;

        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = "application/octet-stream";
        public string? Description { get; set; }
    }
}
