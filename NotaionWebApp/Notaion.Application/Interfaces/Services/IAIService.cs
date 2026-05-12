using System.Collections.Generic;
using System.Threading.Tasks;

namespace Notaion.Application.Interfaces.Services
{
    public enum ChatRole
    {
        User,
        Assistant
    }

    public record ChatTurn(ChatRole Role, string Content);

    public interface IAIService
    {
        Task<string> GetAIResponseAsync(string userMessage);
        Task<string> GetAIResponseAsync(IReadOnlyList<ChatTurn> conversation);
        Task UpdateAIMemoryAsync(string content);
    }
}
