using System;
using System.ComponentModel.DataAnnotations;

namespace Notaion.Domain.Entities
{
    public class FileMetadata
    {
        [Key]
        public Guid Id { get; set; }
        public string OriginalName { get; set; } = string.Empty;
        public string SavedName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long SizeInBytes { get; set; }
        public DateTime UploadedAt { get; set; }
        public string? UserId { get; set; }
        public string? CloudUrl { get; set; } // For Cloudinary migration later
    }
}
