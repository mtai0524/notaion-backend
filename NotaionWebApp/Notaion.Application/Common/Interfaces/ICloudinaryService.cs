using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notaion.Application.Common.Interfaces
{
    public class CloudinaryUploadResult
    {
        public string Url { get; set; } = string.Empty;
        public string PublicId { get; set; } = string.Empty;
        public string ResourceType { get; set; } = string.Empty;
    }

    public interface ICloudinaryService
    {
        Task<string> UploadImageAsync(IFormFile imageFile);
        Task<CloudinaryUploadResult> UploadFileAsync(IFormFile file);
        Task<bool> DeleteFileAsync(string publicId, string resourceType);
    }
}
