using SkillHubAI_Api.Models;

namespace SkillHubAI_Api.Services.Agent
{
    public interface IAgentService
    {
        Task<(string SessionId, AgentSessionInfo Info)> CreateSessionAsync(CancellationToken cancellationToken = default);
        Task<string> ChatAsync(string sessionId, string userMessage, CancellationToken cancellationToken = default);
        Task<List<ChatCitation>> GetLastCitationsAsync();
        Task<AgentSessionInfo?> GetSessionInfoAsync(string sessionId, CancellationToken cancellationToken = default);
    }
}
