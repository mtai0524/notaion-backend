namespace Notaion.Application.DTOs.English
{
    public class EnglishCheckDto
    {
        public string? Text { get; set; }
    }

    public class EnglishCheckResultDto
    {
        public string Corrected { get; set; } = string.Empty;
        public List<string> Notes { get; set; } = new();
        public int Score { get; set; }
    }

    public class EnglishBulkResultDto
    {
        public int Saved { get; set; }
    }
}
