using Microsoft.AspNetCore.Http;
using Notaion.Domain.Entities;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Notaion.Application.Interfaces.Services
{
    public interface IFileService
    {
        Task<FileMetadata> UploadAsync(IFormFile file, string userId);
        Task<FileMetadata> UploadCloudAsync(IFormFile file, string userId);

        /// <summary>Chỉ file do <paramref name="userId"/> upload.</summary>
        Task<List<FileMetadata>> GetAllAsync(string userId);

        /// <remarks>
        /// Không nhận userId: link download được nhúng thẳng vào nội dung note
        /// (markdown `[tên](url)`, `&lt;img src&gt;`) nên không gắn được header
        /// Authorization. Bảo vệ dựa trên `savedName` là GUID không đoán được.
        /// </remarks>
        Task<(Stream stream, string contentType, string originalName)> DownloadAsync(string savedName);

        /// <summary>Xoá file của <paramref name="userId"/>. File của người khác
        /// trả về false giống như không tồn tại, để không lộ sự tồn tại của nó.</summary>
        Task<bool> DeleteAsync(string savedName, string userId);
    }
}
