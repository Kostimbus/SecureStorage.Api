namespace SecureStorage.Core.Interfaces
{
    public interface IFileCryptoService
    {
        Task<byte[]> EncryptAsync(
            byte[] plaintext,
            byte[] salt,
            CancellationToken ct = default);

        Task<byte[]> DecryptAsync(
            byte[] encrypted,
            byte[] salt,
            CancellationToken ct = default);
    }
}
