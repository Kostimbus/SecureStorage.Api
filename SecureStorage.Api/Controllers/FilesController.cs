using System;
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

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] string ownerId)
        {
            if (file == null) return BadRequest("file required");
            if (!Guid.TryParse(ownerId, out var owner)) return BadRequest("ownerId invalid");

            using var ms = new System.IO.MemoryStream();
            await file.CopyToAsync(ms);
            var id = await _files.SaveEncryptedAsync(ms.ToArray(), file.FileName, file.ContentType, owner);
            return CreatedAtAction(nameof(Download), new { id }, new { id });
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Download(Guid id)
        {
            var (record, plaintext) = await _files.GetDecryptedAsync(id);
            if (record == null) return NotFound();
            return File(plaintext, record.ContentType, record.FileName);
        }
    }
}
