using Notaion.Application.DTOs.English;

namespace Notaion.Application.Interfaces.Services
{
    public interface IEnglishDailyService
    {
        // Creates the user's lesson for `date` (yyyy-MM-dd) unless it exists.
        // Null when the AI reply could not be read as a lesson.
        Task<EnglishDailyResultDto?> CreateAsync(string userId, string date);
    }
}
