using System;
using System.Threading;
using System.Threading.Tasks;
using SecureStorage.Core.Models;

namespace SecureStorage.Application.Interfaces
{
    public interface IUserService
    {
        Task<Guid> CreateUserAsync(User user, string passwordPlaintext, CancellationToken ct = default);
        Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
        Task<User?> ValidateCredentialsAsync(string username, string passwordPlaintext, CancellationToken ct = default);
    }
}
