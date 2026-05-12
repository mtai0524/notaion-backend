using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Notaion.Application.Interfaces.Services;
using Notaion.Domain.Entities;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Notaion.Controllers
{
    [ApiController]
    [Route("api/files")]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpPost("upload")]
        public async Task<ActionResult<List<FileMetadata>>> Upload(List<IFormFile> files)
        {
            var results = new List<FileMetadata>();
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var metadata = await _fileService.UploadAsync(file);
                    results.Add(metadata);
                }
            }
            return Ok(results);
        }

        [HttpGet]
        public async Task<ActionResult<List<FileMetadata>>> GetAll()
        {
            var files = await _fileService.GetAllAsync();
            return Ok(files);
        }

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
            var result = await _fileService.DeleteAsync(savedName);
            if (!result) return NotFound();
            return NoContent();
        }
    }
}
