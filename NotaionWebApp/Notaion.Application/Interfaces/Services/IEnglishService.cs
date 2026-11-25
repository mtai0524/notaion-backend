using Notaion.Application.DTOs.English;

namespace Notaion.Application.Interfaces.Services
{
    public interface IEnglishService
    {
        // Null when the AI reply could not be read as a correction.
        Task<EnglishCheckResultDto?> CheckAsync(string text);

        // Null when the AI reply has no meaning.
        Task<EnglishTranslateResultDto?> TranslateAsync(string word, string? context);
    }
}
