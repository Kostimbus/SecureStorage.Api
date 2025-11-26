using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")] // only admins
public class AdminController : ControllerBase
{
    [HttpGet("info")]
    public IActionResult Info() => Ok("Admin area");
}
