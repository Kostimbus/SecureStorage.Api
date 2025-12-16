using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureStorage.Application.Interfaces;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")] // only admins
public class AdminController : ControllerBase
{

    private readonly IUserService _userService;
    public AdminController(IUserService userService) => _userService = userService;

    // <summary>
    /// Test for admin area.
    /// </summary>
    [HttpGet("info")]
    public IActionResult Info() => Ok("Admin area");

    // <summary>
    /// Promote a user to the "Admin" role.
    /// Only accessible by existing Admin users.
    /// </summary>
    /// <param name="username">username of the user to promote</param>

    [HttpPost("promote/{username}")]
    public async Task<IActionResult> Promote(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return BadRequest("username is required");
        }

        var success = await _userService.PromoteToAdminAsync(username);
        if (!success)
        {
            return NotFound($"User '{username}' not found.");
        }

        return NoContent(); // 204 - success
    }
}
