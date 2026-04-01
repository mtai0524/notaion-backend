namespace Notaion.Application.Interfaces.Services
{
    public interface IAIService
    {
        Task<string> GetAIResponseAsync(string userMessage);
    }
}
