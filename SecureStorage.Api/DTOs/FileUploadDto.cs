namespace SecureStorage.Api.DTOs
{
    public class FileUploadDto
    {
        public string? OwnerId { get; set; } // GUID as string; in real app use auth to get owner
    }
}