using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureStorage.Api.DTOs;
using SecureStorage.Application.Interfaces;
using SecureStorage.Core.Interfaces;
using SecureStorage.Infrastructure.Repositories;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")] // only admins
public class AdminController : ControllerBase
{

    private readonly IUserService _userService;
    private readonly IFileRepository _fileRepository;
    public AdminController(IUserService userService, IFileRepository fileRepository) 
    {
        _fileRepository = fileRepository;
        _userService = userService;
    }

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

    /// <summary>
    /// List all files (paged) with owner metadata. Admin only.
    /// Query params: page (default 1), pageSize (default 50).
    /// </summary>
    [HttpGet("files")]
    public async Task<IActionResult> ListAllFiles([FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken ct = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 50 : Math.Min(pageSize, 500);

        var (items, totalCount) = await _fileRepository.ListAllWithOwnerAsync(page, pageSize, ct); 

        var dtoItems = items.Select(f => new FileAdminDto
        {
            Id = f.FileRecord.Id,
            Filename = f.FileRecord.FileName,
            ContentType = f.FileRecord.ContentType,
            PlaintextSize = f.FileRecord.PlaintextSize,
            EncryptedSize = f.FileRecord.EncryptedSize,
            CreatedAtUtc = f.FileRecord.CreatedAtUtc,
            UpdatedAtUtc = f.FileRecord.UpdatedAtUtc,
            Description = f.FileRecord.Description,

            OwnerId = f.Owner.Id,
            OwnerUsername = f.Owner.Username,
            OwnerEmail = f.Owner.Email,
            OwnerIsDisabled = f.Owner.IsDisabled
        }).ToList();

        var result = new
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = dtoItems,
        };

        return Ok(result);
    }
}
