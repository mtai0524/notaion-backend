using System;

namespace Notaion.Domain.Models
{
    public class FileMetadata
    {
        public Guid Id { get; set; }
        public string OriginalName { get; set; }
        public string SavedName { get; set; }
        public string ContentType { get; set; }
        public long SizeInBytes { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
