using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureStorage.Api.DTOs;
using SecureStorage.Application.DTOs;
using SecureStorage.Application.Interfaces;
using SecureStorage.Core.Models;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace SecureStorage.Api.Controllers
{
    [ApiController]
    [Route("api/files")]
    [Authorize]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly IAuditService _auditService;

        public FilesController(IFileService fileService, IAuditService auditService)
        {
            _fileService = fileService;
            _auditService = auditService;

        }

        /// <summary>
        /// Upload a file.
        /// POST /api/files/upload
        /// </summary>
        [HttpPost("upload")]
        [RequestSizeLimit(50 * 1024 * 1024)] // 50MB
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] FileUploadDto dto, CancellationToken ct)
        {
            if (dto?.File == null) return BadRequest("File is required");
            if (dto.File == null || dto.File.Length == 0) return BadRequest("File is required");
            if (!Guid.TryParse(dto.OwnerId, out var owner)) return BadRequest("ownerId is invalid");

            // Read file into memory stream (ok для <= 50MB).
            var ms = new MemoryStream();
            await dto.File.CopyToAsync(ms, ct);
            ms.Position = 0; // important

            var request = new UploadFileRequest
            {
                Content = ms,
                FileName = dto.File.FileName,
                ContentType = dto.File.ContentType ?? "application/octet-stream",
                Description = dto.Description
            };

            var ownerId = GetUserIdFromClaims();
            var result = await _fileService.UploadAsync(request, dto.Description, ownerId, ct);

            // log audit entry
            var actorId = GetActorUserId();
            var actorUsername = User.Identity?.Name ?? GetClaim(ClaimTypes.Name) ?? GetClaim(ClaimTypes.Upn) ?? GetClaim("preferred_username");
            var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();

            await _auditService.LogEventAsync(
                AuditEventType.Upload,
                actorId,
                actorUsername,
                details: $"FileName={dto.File.FileName};Size={result.EncryptedSize}",
                result.Id,
                remoteIp: remoteIp
            );

            return CreatedAtAction(nameof(Download), new { id = result.Id }, result);

        }

        /// <summary>
        /// Download file by id. Returns decrypted plaintext bytes.
        /// GET /api/files/{id}
        /// </summary>
        [HttpGet("{id:guid}/download")]
        public async Task<IActionResult> Download([FromRoute] Guid id, CancellationToken ct)
        {
            var requestorId = GetUserIdFromClaims();
            var (record, plaintext) = await _fileService.DownloadAsync(id, requestorId, ct);

            // log download (actor may be null if anonymous - but we require auth above)
            var actorId = GetActorUserId();
            var actorUsername = User.Identity?.Name ?? GetClaim(ClaimTypes.Name);

            await _auditService.LogEventAsync(
                AuditEventType.Download,
                actorId,
                actorUsername,
                details: $"FileName={record.FileName};Size={record.PlaintextSize}",
                id,
                remoteIp: HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            return File(plaintext, record.ContentType ?? "application/octet-stream", record.FileName);
        }

        /// <summary>
        /// Delete a file (owner or admin).
        /// </summary>
        [HttpDelete("{id:guid}/delete")]
        [Authorize] // owner or admin required
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
        {
            // check ownership or admin role
            var actorId = GetActorUserId();
            if (actorId == null)
                return Unauthorized();

            var actorUsername = User.Identity?.Name ?? GetClaim(ClaimTypes.Name);
            var isAdmin = User.IsInRole("Admin");

            var requestorId = GetUserIdFromClaims();

            if (!isAdmin)
            {
                var isOwner = await _fileService.IsOwnerAsync(id, actorId.Value, ct);
                if (!isOwner)
                    return Forbid();
            }

            await _fileService.DeleteAsync(id, requestorId, ct);

            // log delete
            await _auditService.LogEventAsync(
                AuditEventType.Delete,
                actorId,
                actorUsername,
                details: $"FileId={id}",
                id,
                remoteIp: HttpContext.Connection.RemoteIpAddress?.ToString()
            );
            return NoContent();
        }

        /// <summary>
        /// List files owned by the authenticated user.
        /// </summary>
        [HttpGet] // GET /api/files
        public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
        {
            var ownerId = GetUserIdFromClaims(); // your helper to extract user ID from JWT

            var files = await _fileService.ListByOwnerAsync(ownerId, page, pageSize, ct);

            var result = files.Select(f => new FileResponseDto
            {
                Id = f.Id,
                FileName = f.FileName,
                ContentType = f.ContentType,
                PlaintextSize = f.PlaintextSize,
                EncryptedSize = f.EncryptedSize,
                CreatedAtUtc = f.CreatedAtUtc,
                Description = f.Description
            });

            return Ok(result);
        }
        // helpers

        private Guid? GetActorUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (idClaim == null) return null;
            return Guid.TryParse(idClaim.Value, out var g) ? g : null;
        }

        private string? GetClaim(string type) => User.FindFirst(type)?.Value;

        private Guid GetUserIdFromClaims()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (claim == null || !Guid.TryParse(claim.Value, out var id))
                throw new UnauthorizedAccessException("User id not found or invalid in token");
            return id;
        }
    }
}
