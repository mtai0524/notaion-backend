using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Notaion.Application.Interfaces.Services;
using Notaion.Domain.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Notaion.Infrastructure.Services
{
    public class FileService : IFileService
    {
        private readonly string _uploadPath;
        private readonly string _metadataPath;
        private static readonly object _lock = new object();

        public FileService(IConfiguration configuration)
        {
            _uploadPath = configuration["FileStorage:UploadPath"] ?? "Uploads";
            if (!Path.IsPathRooted(_uploadPath))
            {
                _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), _uploadPath);
            }

            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }

            _metadataPath = Path.Combine(_uploadPath, "metadata.json");
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

            SaveMetadata(metadata);

            return metadata;
        }

        public async Task<List<FileMetadata>> GetAllAsync()
        {
            return LoadAllMetadata();
        }

        public async Task<(Stream stream, string contentType, string originalName)> DownloadAsync(string savedName)
        {
            var allMetadata = LoadAllMetadata();
            var metadata = allMetadata.FirstOrDefault(m => m.SavedName == savedName);

            if (metadata == null)
            {
                throw new FileNotFoundException("File metadata not found.");
            }

            var filePath = Path.Combine(_uploadPath, savedName);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Physical file not found.");
            }

            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return (stream, metadata.ContentType, metadata.OriginalName);
        }

        public async Task<bool> DeleteAsync(string savedName)
        {
            var allMetadata = LoadAllMetadata();
            var metadata = allMetadata.FirstOrDefault(m => m.SavedName == savedName);

            if (metadata == null) return false;

            var filePath = Path.Combine(_uploadPath, savedName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            allMetadata.RemoveAll(m => m.SavedName == savedName);
            SaveAllMetadata(allMetadata);

            return true;
        }

        private List<FileMetadata> LoadAllMetadata()
        {
            lock (_lock)
            {
                if (!File.Exists(_metadataPath)) return new List<FileMetadata>();

                var json = File.ReadAllText(_metadataPath);
                return JsonSerializer.Deserialize<List<FileMetadata>>(json) ?? new List<FileMetadata>();
            }
        }

        private void SaveMetadata(FileMetadata metadata)
        {
            lock (_lock)
            {
                var allMetadata = LoadAllMetadata();
                allMetadata.Add(metadata);
                SaveAllMetadata(allMetadata);
            }
        }

        private void SaveAllMetadata(List<FileMetadata> allMetadata)
        {
            lock (_lock)
            {
                var json = JsonSerializer.Serialize(allMetadata, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_metadataPath, json);
            }
        }
    }
}
