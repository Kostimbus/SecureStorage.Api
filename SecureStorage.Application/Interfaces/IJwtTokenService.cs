using SecureStorage.Core.Models;

namespace SecureStorage.Application.Interfaces
{
    public interface IJwtTokenService
    {
        /// <summary>
        /// Generate a signed JWT for the provided user.
        /// </summary>
        string GenerateToken(User user);
    }
}
