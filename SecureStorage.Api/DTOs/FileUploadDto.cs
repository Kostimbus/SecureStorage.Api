using Microsoft.AspNetCore.Http;

namespace SecureStorage.Api.DTOs
{
    /// <summary>
    /// DTO for handling file uploads via multipart/form-data.
    /// Use [FromForm] binding with this DTO so Swagger can generate the schema correctly.
    /// </summary>
    public class FileUploadDto
    {
        /// <summary>
        /// The uploaded file.
        /// </summary>
        public IFormFile File { get; set; } = default!;

        /// <summary>
        /// Owner id as string (will be parsed to Guid in controller).
        /// </summary>
        public string OwnerId { get; set; } = string.Empty;

        /// <summary>
        /// Optional description / metadata.
        /// </summary>
        public string? Description { get; set; }
    }
}
