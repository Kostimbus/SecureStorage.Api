using Microsoft.Extensions.Options;
using SecureStorage.Core.Interfaces;
using SecureStorage.Infrastructure.Crypto;
using SecureStorage.Infrastructure.Options;
using System.Security.Cryptography;

public sealed class AesGcmFileCryptoService : IFileCryptoService
{
    private readonly byte[] _masterKey;
    private readonly FileStorageOptions _opts;
    private readonly AesGcmFileEncryptionService _aes;

    public AesGcmFileCryptoService(
        AesGcmFileEncryptionService aes,
        IOptions<AesGcmOptions> aesOptions,
        IOptions<FileStorageOptions> fileOptions)
    {
        _aes = aes;
        _opts = fileOptions.Value;
        _masterKey = Convert.FromBase64String(aesOptions.Value.Base64Key);
    }

    public async Task<byte[]> EncryptAsync(byte[] plaintext, byte[] salt, CancellationToken ct)
    {
        var key = DeriveKey(salt);
        return await _aes.EncryptWithKeyAsync(key, plaintext, null, ct);
    }

    public async Task<byte[]> DecryptAsync(byte[] encrypted, byte[] salt, CancellationToken ct)
    {
        var key = DeriveKey(salt);
        return await _aes.DecryptWithKeyAsync(key, encrypted, null, ct);
    }

    private byte[] DeriveKey(byte[] salt)
    {
        return Rfc2898DeriveBytes.Pbkdf2(
            _masterKey,             // password/input
            salt,                   // salt
            _opts.Pbkdf2Iterations, // iterations
            HashAlgorithmName.SHA256, // hash algorithm
            32);                    // output length (bytes)
    }
}
