using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Notaion.Application.Common.Interfaces;
using Notaion.Infrastructure.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notaion.Infrastructure.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinaryOptions> options)
        {
            var account = new Account(
                options.Value.CloudName,
                options.Value.ApiKey,
                options.Value.ApiSecret
            );
            _cloudinary = new Cloudinary(account);
        }

        [RequestSizeLimit(1024 * 1024 * 100)]
        public async Task<string> UploadImageAsync(IFormFile imageFile)
        {
            try
            {
                if (imageFile == null || imageFile.Length == 0)
                {
                    throw new ArgumentException("Không có hình ảnh hoặc ảnh không tồn tại.");
                }

                using (var stream = imageFile.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(imageFile.FileName, stream),
                    };

                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                    if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return uploadResult.SecureUrl.AbsoluteUri;
                    }
                    else
                    {
                        throw new Exception("Failed to upload image to Cloudinary.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error uploading image to Cloudinary: " + ex.Message);
                return null;
            }
        }

        [RequestSizeLimit(1024 * 1024 * 100)]
        public async Task<CloudinaryUploadResult> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File rỗng hoặc không tồn tại.");
            }

            var contentType = file.ContentType ?? string.Empty;
            using var stream = file.OpenReadStream();
            var description = new FileDescription(file.FileName, stream);

            RawUploadResult uploadResult;
            string resourceType;

            if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                resourceType = "image";
                uploadResult = await _cloudinary.UploadAsync(new ImageUploadParams { File = description });
            }
            else if (contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            {
                resourceType = "video";
                uploadResult = await _cloudinary.UploadAsync(new VideoUploadParams { File = description });
            }
            else
            {
                resourceType = "raw";
                uploadResult = await _cloudinary.UploadAsync(new RawUploadParams { File = description });
            }

            if (uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception($"Cloudinary upload thất bại: {uploadResult.Error?.Message}");
            }

            return new CloudinaryUploadResult
            {
                Url = uploadResult.SecureUrl.AbsoluteUri,
                PublicId = uploadResult.PublicId,
                ResourceType = resourceType
            };
        }

        public async Task<bool> DeleteFileAsync(string publicId, string resourceType)
        {
            if (string.IsNullOrWhiteSpace(publicId)) return false;

            var deletionParams = new DeletionParams(publicId)
            {
                ResourceType = resourceType?.ToLowerInvariant() switch
                {
                    "video" => ResourceType.Video,
                    "raw" => ResourceType.Raw,
                    _ => ResourceType.Image
                }
            };

            var result = await _cloudinary.DestroyAsync(deletionParams);
            return result.Result == "ok";
        }
    }
}
