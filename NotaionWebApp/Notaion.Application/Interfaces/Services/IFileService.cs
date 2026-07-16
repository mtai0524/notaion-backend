using Microsoft.AspNetCore.Http;
using Notaion.Domain.Entities;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Notaion.Application.Interfaces.Services
{
    public interface IFileService
    {
        Task<FileMetadata> UploadAsync(IFormFile file);
        Task<FileMetadata> UploadCloudAsync(IFormFile file);
        Task<List<FileMetadata>> GetAllAsync();
        Task<(Stream stream, string contentType, string originalName)> DownloadAsync(string savedName);
        // Proxy-download a Cloudinary-hosted file server-side (avoids the browser
        // 401 Cloudinary returns for restricted media like PDF/ZIP).
        Task<(Stream stream, string contentType, string originalName)> DownloadCloudAsync(string savedName);
        Task<bool> DeleteAsync(string savedName);
    }
}
