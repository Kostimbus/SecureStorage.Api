using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SecureStorage.Application.Interfaces;
using SecureStorage.Api.DTOs;
using SecureStorage.Core.Models;

namespace SecureStorage.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _users;

        public UsersController(IUserService users) => _users = users;

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserCreateDto dto)
        {
            var user = new User
            {
                Username = dto.Username.Trim(),
                Email = dto.Email.Trim(),
                DisplayName = dto.DisplayName
            };

            var id = await _users.CreateUserAsync(user, dto.Password);
            return CreatedAtAction(null, new { id }, null);
        }
    }
}
