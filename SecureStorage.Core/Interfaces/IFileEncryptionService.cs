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
        /// <param name="plaintext">The raw bytes to encrypt.</param>
        /// <param name="associatedData">Optional associated data for AEAD.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Encrypted bytes.</returns>
        Task<byte[]> EncryptAsync(byte[] plaintext, byte[]? associatedData = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Decrypts previously encrypted payload returned by <see cref="EncryptAsync"/>.
        /// </summary>
        /// <param name="encryptedPayload">Encrypted payload produced by the encryptor.</param>
        /// <param name="associatedData">Optional associated data that was used when encrypting.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Decrypted plaintext bytes.</returns>
        Task<byte[]> DecryptAsync(byte[] encryptedPayload, byte[]? associatedData = null, CancellationToken cancellationToken = default);
    }
}
