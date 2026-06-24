/*using SkillHubAI_Api.Models;

namespace SkillHubAI_Api.Services.Chat
{
    public interface IChatSessionService
    {
        Task<ChatSession> CreateSessionAsync(CancellationToken cancellationToken = default);
        Task<ChatSession?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default);
        Task<List<ChatSession>> GetAllSessionsAsync(CancellationToken cancellationToken = default);
        Task SaveMessageAsync(ChatMessage message, CancellationToken cancellationToken = default);
        Task<List<ChatMessage>> GetRecentMessagesAsync(string sessionId, int count = 10, CancellationToken cancellationToken = default);
        Task UpdateSessionAsync(ChatSession session, CancellationToken cancellationToken = default);
    }
}
*/