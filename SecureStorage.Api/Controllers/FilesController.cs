using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureStorage.Api.DTOs;
using SecureStorage.Application.Interfaces;
using SecureStorage.Application.DTOs;

namespace SecureStorage.Api.Controllers
{
    [ApiController]
    [Route("api/files")]
    [Authorize]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FilesController(IFileService fileService) => _fileService = fileService;

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
            return File(plaintext, record.ContentType, record.FileName);
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
        // helper
        private Guid GetUserIdFromClaims()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
            if (claim == null || !Guid.TryParse(claim.Value, out var id))
                throw new UnauthorizedAccessException("User id not found or invalid in token");
            return id;
        }
    }
}
