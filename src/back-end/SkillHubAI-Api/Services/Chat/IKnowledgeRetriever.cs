using SkillHubAI_Api.Models;

namespace SkillHubAI_Api.Services.Chat
{
    public interface IKnowledgeRetriever
    {
        Task<List<ChatCitation>> RetrieveAsync(
          string query,
          int topK = 5,
          CancellationToken cancellationToken = default);
    }
}
