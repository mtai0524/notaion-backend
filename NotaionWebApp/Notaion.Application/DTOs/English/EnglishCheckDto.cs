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

    public class EnglishDailyDto
    {
        // The learner's local date, yyyy-MM-dd.
        public string? Date { get; set; }
    }

    public class EnglishDailyResultDto
    {
        // False when the lesson for that date already existed.
        public bool Created { get; set; }
        public string? Topic { get; set; }
        public int Cards { get; set; }
        public int Sentences { get; set; }
    }
}
