using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SecureStorage.Application.Interfaces;
using SecureStorage.Core.Interfaces;
using SecureStorage.Core.Models;
using Microsoft.AspNetCore.Identity;

namespace SecureStorage.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly PasswordHasher<User> _hasher = new();

        public UserService(IUserRepository userRepo)
        {
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
        }

        // Change after on PBKDF2/Argon2
        private static string HashPassword(string password)
        {
            // NOT for production.
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        public async Task<Guid> CreateUserAsync(User user, string passwordPlaintext, CancellationToken ct = default)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(passwordPlaintext)) throw new ArgumentException("Password cannot be null or whitespace.", nameof(passwordPlaintext));
            user.PasswordHash = _hasher.HashPassword(user, passwordPlaintext);
            return await _userRepo.CreateAsync(user, ct).ConfigureAwait(false);
        }

        public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
            => await _userRepo.GetByUsernameAsync(username, ct);

        public async Task<User?> ValidateCredentialsAsync(string username, string passwordPlaintext, CancellationToken ct = default)
        {
            var user = await _userRepo.GetByUsernameAsync(username, ct);
            if (user == null) return null;
            var res = _hasher.VerifyHashedPassword(user, user.PasswordHash ?? string.Empty, passwordPlaintext);
            return res == PasswordVerificationResult.Success ? user : null;
        }
    }
}
