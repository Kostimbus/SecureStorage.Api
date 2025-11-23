using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SecureStorage.Core.Interfaces;

namespace SecureStorage.Infrastructure.Crypto
{
    /// <summary>
    /// AES-GCM encryptor implementation. Uses format: nonce(12) | tag(16) | ciphertext.
    /// The key must be 32 bytes (AES-256) passed as Base64 in configuration "Encryption:Base64Key".
    /// </summary>
    
    public class AesGcmOptions
    {
        public string? Base64Key { get; set; }
    }

    public class AesGcmFileEncryptionService : IFileEncryptionService, IDisposable
    {
        private readonly byte[] _key;

        public AesGcmFileEncryptionService(IOptions<AesGcmOptions> options)
        {
            var opts = options?.Value ?? throw new ArgumentNullException(nameof(options));
            if (string.IsNullOrWhiteSpace(opts.Base64Key))
            {
                throw new InvalidOperationException("Encryption key is not configured. Set Encryption:Base64Key.");
            }
            _key = Convert.FromBase64String(opts.Base64Key);
            if (_key.Length != 32)
            {
                throw new InvalidOperationException("Encryption key is not configured. Set Encryption:Base64Key.");
            }
        }

        public Task<byte[]> EncryptAsync(byte[] plaintext, byte[]? associatedData = null, CancellationToken cancellationToken = default)
        {
            if (plaintext == null) throw new ArgumentNullException(nameof(plaintext));

            var nonce = new byte[12];
            RandomNumberGenerator.Fill(nonce);

            var ciphertext = new byte[plaintext.Length];
            var tag = new byte[16];

            using var aes = new AesGcm(_key, 16);
            aes.Encrypt(nonce, plaintext, ciphertext, tag, associatedData);

            var combined = new byte[nonce.Length + tag.Length + ciphertext.Length];
            Buffer.BlockCopy(nonce, 0, combined, 0, nonce.Length);
            Buffer.BlockCopy(tag, 0, combined, nonce.Length, tag.Length);
            Buffer.BlockCopy(ciphertext, 0, combined, nonce.Length + tag.Length, ciphertext.Length);

            return Task.FromResult(combined);
        }

        public Task<byte[]> DecryptAsync(byte[] encryptedPayload, byte[]? associatedData = null, CancellationToken cancellationToken = default)
        {
            if (encryptedPayload == null) throw new ArgumentNullException(nameof(encryptedPayload));
            if (encryptedPayload.Length < 28) throw new ArgumentException("Invalid encrypted payload", nameof(encryptedPayload));

            var nonce = new byte[12];
            var tag = new byte[16];
            var ciphertext = new byte[encryptedPayload.Length - 28];

            Buffer.BlockCopy(encryptedPayload, 0, nonce, 0, nonce.Length);
            Buffer.BlockCopy(encryptedPayload, nonce.Length, tag, 0, tag.Length);
            Buffer.BlockCopy(encryptedPayload, nonce.Length + tag.Length, ciphertext, 0, ciphertext.Length);

            var plaintext = new byte[ciphertext.Length];
            using var aes = new AesGcm(_key, 16);
            aes.Decrypt(nonce, ciphertext, tag, plaintext, associatedData);

            return Task.FromResult(plaintext);
        }

        public void Dispose()
        {
            // Clear the key from memory
            if (_key != null)
            {
                Array.Clear(_key, 0, _key.Length);
            }
            GC.SuppressFinalize(this);
        }

    }
}
