using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SecureStorage.Application.Interfaces;
using SecureStorage.Api.DTOs;

namespace SecureStorage.Api.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _files;

        public FilesController(IFileService files) => _files = files;

        /// <summary>
        /// Upload a file (multipart/form-data).  
        /// Use FileUploadDto so Swashbuckle/Swagger can generate the request correctly.
        /// </summary>
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] FileUploadDto dto)
        {
            if (dto == null) return BadRequest("request body is required");
            if (dto.File == null || dto.File.Length == 0) return BadRequest("file required");

            if (!Guid.TryParse(dto.OwnerId, out var owner))
                return BadRequest("ownerId invalid");

            // Read file bytes (for demo). For large files consider streaming to storage instead.
            byte[] fileBytes;
            await using (var ms = new MemoryStream())
            {
                await dto.File.CopyToAsync(ms);
                fileBytes = ms.ToArray();
            }

            // Save encrypted bytes via your application/service. Returns the created file id.
            var id = await _files.SaveEncryptedAsync(fileBytes, dto.File.FileName, dto.File.ContentType, owner);

            // Return Created pointing to Download endpoint
            return CreatedAtAction(nameof(Download), new { id }, new { id });
        }

        /// <summary>
        /// Download file by id. Returns decrypted plaintext bytes.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Download(Guid id)
        {
            var (record, plaintext) = await _files.GetDecryptedAsync(id);

            if (record == null) return NotFound();

            // plaintext may be null if underlying service failed — handle gracefully
            if (plaintext == null || plaintext.Length == 0) return NotFound();

            var contentType = string.IsNullOrWhiteSpace(record.ContentType)
                ? "application/octet-stream"
                : record.ContentType;

            // File(byte[] fileContents, string contentType, string? fileDownloadName)
            return File(plaintext, contentType, record.FileName);
        }
    }
}
