using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SecureStorage.Api.DTOs;
using SecureStorage.Application.Interfaces;
using SecureStorage.Application.Services;
using SecureStorage.Core.Models;
using System;
using System.Threading.Tasks;

namespace SecureStorage.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtTokenService _tokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, IJwtTokenService tokenService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _tokenService = tokenService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AuthRegisterDto dto)
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Username and password are required.");

            // check if exists
            var existing = await _userService.GetByUsernameAsync(dto.Username);
            if (existing != null) return Conflict("Username already taken.");

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                // Role defaults to "User"
            };

            var id = await _userService.CreateUserAsync(user, dto.Password);
            _logger.LogInformation("Created user {UserId}", id);
            return CreatedAtAction(null, new { id });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthLoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Username and password are required.");
            var user = await _userService.ValidateCredentialsAsync(dto.Username, dto.Password);
            if (user == null) return Unauthorized("Invalid credentials.");

            // read expiration info from config? For now compute by +ExpireMinutes from config is embedded in token
            // We just return token and role
            var token = _tokenService.GenerateToken(user);
            var response = new AuthResponseDto
            {
                Token = token,
                Username = user.Username,
                Role = user.Role ?? "User",
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(60) // keep in sync with Jwt:ExpireMinutes if you change it
            };

            return Ok(response);
        }
    }
}
