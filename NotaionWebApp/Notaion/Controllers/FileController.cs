using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Notaion.Application.Interfaces.Services;
using Notaion.Domain.Entities;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Notaion.Controllers
{
    [ApiController]
    [Route("api/files")]
    // Scheme chỉ định rõ vì app đăng ký cả cookie (OAuth Discord/GitHub) lẫn JWT;
    // frontend và desktop đều gửi Bearer token (axiosConfig.js, desktop/api.js).
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpPost("upload")]
        public async Task<ActionResult<List<FileMetadata>>> Upload(List<IFormFile> files)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var results = new List<FileMetadata>();
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var metadata = await _fileService.UploadAsync(file, userId);
                    results.Add(metadata);
                }
            }
            return Ok(results);
        }

        [HttpPost("upload/cloudinary")]
        public async Task<ActionResult<List<FileMetadata>>> UploadToCloudinary(List<IFormFile> files)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var results = new List<FileMetadata>();
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var metadata = await _fileService.UploadCloudAsync(file, userId);
                    results.Add(metadata);
                }
            }
            return Ok(results);
        }

        [HttpGet]
        public async Task<ActionResult<List<FileMetadata>>> GetAll()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var files = await _fileService.GetAllAsync(userId);
            return Ok(files);
        }

        // Mở cho mọi người: URL này được nhúng thẳng vào nội dung note dưới dạng
        // markdown/`<img src>` (Notion.jsx, DailyNoteApp.jsx, desktop/attachments.js)
        // nên trình duyệt không gắn được header Authorization. `savedName` là GUID
        // nên link không đoán được — cùng mức bảo vệ như URL Cloudinary vốn đã
        // public hoàn toàn. Bắt buộc đăng nhập ở đây sẽ làm hỏng mọi file đính kèm
        // đã nhúng trong note cũ.
        [AllowAnonymous]
        [HttpGet("download/{savedName}")]
        public async Task<IActionResult> Download(string savedName)
        {
            try
            {
                var (stream, contentType, originalName) = await _fileService.DownloadAsync(savedName);
                return File(stream, contentType, originalName);
            }
            catch (FileNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{savedName}")]
        public async Task<IActionResult> Delete(string savedName)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _fileService.DeleteAsync(savedName, userId);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
