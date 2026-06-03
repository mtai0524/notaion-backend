namespace Notaion.Domain.Entities
{
    /// <summary>
    /// A file/image attached to a DailyNote. Stored as a JSON string column
    /// (see ApplicationDbContext value converter) so the list round-trips with
    /// the note without needing a separate table. URLs point at Cloudinary.
    /// </summary>
    public class Attachment
    {
        public string? Type { get; set; }        // "image" | "file"
        public string? Url { get; set; }          // Cloudinary secure URL
        public string? Name { get; set; }         // original file name
        public long? Size { get; set; }           // size in bytes
        public string? ContentType { get; set; }  // MIME type
    }
}
