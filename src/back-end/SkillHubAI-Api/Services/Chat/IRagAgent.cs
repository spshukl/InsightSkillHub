/*using SkillHubAI_Api.Models;

namespace SkillHubAI_Api.Services.Chat
{
    public interface IRagAgent
    {
        IAsyncEnumerable<string> ChatStreamAsync(
            string userMessage,
            List<Models.ChatMessage> chatHistory,
            CancellationToken cancellationToken = default);
        Task<string> ChatAsync(
            string userMessage,
            List<Models.ChatMessage> chatHistory,
            CancellationToken cancellationToken = default);


        List<ChatCitation> GetLastCitations();
    }
}
*/