using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SecureStorage.Application.Interfaces;
using SecureStorage.Core.Interfaces;
using SecureStorage.Core.Models;

namespace SecureStorage.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;

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
            user.PasswordHash = HashPassword(passwordPlaintext);
            return await _userRepo.CreateAsync(user, ct).ConfigureAwait(false);
        }

        public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
            => _userRepo.GetByUsernameAsync(username, ct);
    }
}
