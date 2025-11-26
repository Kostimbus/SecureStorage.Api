namespace SecureStorage.Infrastructure.Options
{
    public class FileStorageOptions
    {
        public string BasePath { get; set; } = "data/files";
        public long MaxFileSizeBytes { get; set; } = 50 * 1024 * 1024;
        public string[] AllowedContentTypes { get; set; } = new[] { "application/pdf", "image/png", "image/jpeg", "text/plain" };
        public int Pbkdf2Iterations { get; set; } = 100_000;
        public int SaltSize { get; set; } = 16;
    }
}
