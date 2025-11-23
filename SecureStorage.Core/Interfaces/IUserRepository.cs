using System;
using System.Threading;
using System.Threading.Tasks;
using SecureStorage.Core.Models;

namespace SecureStorage.Core.Interfaces
{
    /// <summary>
    /// Repository abstraction for user persistence and lookup.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Create a user
        /// </summary>
        Task<Guid> CreateAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a user by id, or null.
        /// </summary>
        Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a user by username (unique), or null.
        /// </summary>
        Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a user by email (unique), or null.
        /// </summary>
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a user record. The user.Id must be set.
        /// </summary>
        Task UpdateAsync(User user, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a user by id. Returns true if deletion happened.
        /// </summary>
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        
    }
}
