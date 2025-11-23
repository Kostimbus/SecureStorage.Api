using System;

namespace SecureStorage.Core.Models
{
    /// <summary>
    /// Domain model for a user in the Secure Storage system.
    /// Keep authentication-related concerns (password hashing, salts) in Infrastructure or in a dedicated Auth service.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Primary id (GUID).
        /// </summary>
        public Guid Id { get; init; } = Guid.NewGuid();

        /// <summary>
        /// Unique username chosen by user (login). Use normalization in repository queries.
        /// </summary>
        public string Username { get; init; } = string.Empty;

        /// <summary>
        /// Email address (prefer unique constraint in DB).
        /// </summary>
        public string Email { get; init; } = string.Empty;

        /// <summary>
        /// Hashed password (or null if this user uses external auth / OAuth).
        /// Hashing algorithm and salt storage implemented in Infrastructure.
        /// </summary>
        public string? PasswordHash { get; set; }

        /// <summary>
        /// When account was created (UTC).
        /// </summary>
        public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

        /// <summary>
        /// When user was last updated (UTC).
        /// </summary>
        public DateTime? UpdatedAtUtc { get; set; }

        /// <summary>
        /// Flag for soft-delete / disabled accounts.
        /// </summary>
        public bool IsDisabled { get; set; }

        /// <summary>
        /// Optional display name.
        /// </summary>
        public string? DisplayName { get; set; }
    }
}
