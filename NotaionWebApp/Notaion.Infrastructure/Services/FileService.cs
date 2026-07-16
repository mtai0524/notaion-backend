using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Notaion.Application.Common.Interfaces;
using Notaion.Application.Interfaces.Services;
using Notaion.Domain.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Notaion.Domain.Entities;
using Notaion.Infrastructure.Context;

namespace Notaion.Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly string _uploadPath;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ICloudinaryService _cloudinaryService;

        public FileService(IConfiguration configuration, IServiceScopeFactory scopeFactory, ICloudinaryService cloudinaryService)
        {
            _scopeFactory = scopeFactory;
            _cloudinaryService = cloudinaryService;
            _uploadPath = configuration["FileStorage:UploadPath"] ?? "Uploads";
            if (!Path.IsPathRooted(_uploadPath))
            {
                _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), _uploadPath);
            }

            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<FileMetadata> UploadAsync(IFormFile file)
        {
            var id = Guid.NewGuid();
            var extension = Path.GetExtension(file.FileName);
            var savedName = $"{id}{extension}";
            var filePath = Path.Combine(_uploadPath, savedName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var metadata = new FileMetadata
            {
                Id = id,
                OriginalName = file.FileName,
                SavedName = savedName,
                ContentType = file.ContentType,
                SizeInBytes = file.Length,
                UploadedAt = DateTime.UtcNow
            };

            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.FileMetadatas.Add(metadata);
                await dbContext.SaveChangesAsync();
            }

            return metadata;
        }

        public async Task<FileMetadata> UploadCloudAsync(IFormFile file)
        {
            var uploadResult = await _cloudinaryService.UploadFileAsync(file);

            var metadata = new FileMetadata
            {
                Id = Guid.NewGuid(),
                OriginalName = file.FileName,
                SavedName = uploadResult.PublicId,
                ContentType = file.ContentType,
                SizeInBytes = file.Length,
                UploadedAt = DateTime.UtcNow,
                CloudUrl = uploadResult.Url
            };

            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                dbContext.FileMetadatas.Add(metadata);
                await dbContext.SaveChangesAsync();
            }

            return metadata;
        }

        public async Task<List<FileMetadata>> GetAllAsync()
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                return await dbContext.FileMetadatas.OrderByDescending(m => m.UploadedAt).ToListAsync();
            }
        }

        public async Task<(Stream stream, string contentType, string originalName)> DownloadAsync(string savedName)
        {
            FileMetadata metadata;
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                metadata = await dbContext.FileMetadatas.FirstOrDefaultAsync(m => m.SavedName == savedName);
            }

            if (metadata == null)
            {
                throw new FileNotFoundException("File metadata not found.");
            }

            if (!string.IsNullOrEmpty(metadata.CloudUrl))
            {
                throw new InvalidOperationException("File này lưu trên Cloudinary, hãy download trực tiếp từ CloudUrl.");
            }

            var filePath = Path.Combine(_uploadPath, savedName);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Physical file not found.");
            }

            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return (stream, metadata.ContentType, metadata.OriginalName);
        }

        private static readonly System.Net.Http.HttpClient _http = new System.Net.Http.HttpClient();

        public async Task<(Stream stream, string contentType, string originalName)> DownloadCloudAsync(string savedName)
        {
            FileMetadata metadata;
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                metadata = await dbContext.FileMetadatas.FirstOrDefaultAsync(m => m.SavedName == savedName);
            }

            if (metadata == null)
            {
                throw new FileNotFoundException("File metadata not found.");
            }
            if (string.IsNullOrEmpty(metadata.CloudUrl))
            {
                throw new InvalidOperationException("File này không lưu trên Cloudinary.");
            }

            // Fetch the file server-side and stream it back. The server request is
            // not subject to the browser CORS/referrer restriction that makes the
            // direct Cloudinary URL 401 in the browser.
            var response = await _http.GetAsync(metadata.CloudUrl);
            response.EnsureSuccessStatusCode();
            var stream = await response.Content.ReadAsStreamAsync();
            var contentType = response.Content.Headers.ContentType?.ToString()
                ?? (string.IsNullOrEmpty(metadata.ContentType) ? "application/octet-stream" : metadata.ContentType);
            return (stream, contentType, metadata.OriginalName);
        }

        public async Task<bool> DeleteAsync(string savedName)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var metadata = await dbContext.FileMetadatas.FirstOrDefaultAsync(m => m.SavedName == savedName);

                if (metadata == null) return false;

                if (!string.IsNullOrEmpty(metadata.CloudUrl))
                {
                    var resourceType = (metadata.ContentType ?? string.Empty).StartsWith("image/", StringComparison.OrdinalIgnoreCase)
                        ? "image"
                        : (metadata.ContentType ?? string.Empty).StartsWith("video/", StringComparison.OrdinalIgnoreCase)
                            ? "video"
                            : "raw";

                    await _cloudinaryService.DeleteFileAsync(metadata.SavedName, resourceType);
                }
                else
                {
                    var filePath = Path.Combine(_uploadPath, savedName);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }

                dbContext.FileMetadatas.Remove(metadata);
                await dbContext.SaveChangesAsync();
            }

            return true;
        }
    }
}
