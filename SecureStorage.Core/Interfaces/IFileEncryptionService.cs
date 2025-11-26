using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureStorage.Core.Interfaces
{
    /// <summary>
    /// Provides symmetric file encryption/decryption operations.
    /// Implementation belongs to Infrastructure
    /// </summary>

    public interface IFileEncryptionService
    {
        /// <summary>
        /// Encrypts the provided plaintext bytes and returns the encrypted payload.
        /// The format of the returned payload is implementation-defined.
        /// </summary>
        Task<byte[]> EncryptAsync(byte[] plaintext, byte[]? associatedData = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Decrypts previously encrypted payload returned by <see cref="EncryptAsync"/>.
        /// </summary>
        Task<byte[]> DecryptAsync(byte[] encryptedPayload, byte[]? associatedData = null, CancellationToken cancellationToken = default);

        Task<byte[]> EncryptWithKeyAsync(byte[] key, byte[] plaintext, byte[]? associatedData = null, CancellationToken cancellationToken = default);

        Task<byte[]> DecryptWithKeyAsync(byte[] key, byte[] encryptedPayload, byte[]? associatedData = null, CancellationToken cancellationToken = default);
    }
}
