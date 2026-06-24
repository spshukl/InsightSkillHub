/*using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using SkillHubAI_Api.Configurations.settings;
using SkillHubAI_Api.Models;
using Microsoft.Azure.Cosmos.Linq;
using AIChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace SkillHubAI_Api.Services.Chat
{
    public sealed class ChatSessionService : IChatSessionService
    {
        private readonly Container _container;
        private readonly ILogger<ChatSessionService> _logger;

        public ChatSessionService(
            CosmosClient cosmosClient,
            IOptions<CosmosDbSettings> settings,
            ILogger<ChatSessionService> logger)
        {
            _logger = logger;
            var db = cosmosClient.GetDatabase(settings.Value.DatabaseName);
            _container = db.GetContainer("ChatHistory");
        }

        public async Task<ChatSession> CreateSessionAsync(CancellationToken cancellationToken = default)
        {
            var session = new ChatSession
            {
                SessionId = Guid.NewGuid().ToString(),
                Title = "New Chat",
                CreatedAt = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow
            };
            session.Id = session.SessionId; // id = sessionId for the session doc

            await _container.CreateItemAsync(session, new PartitionKey(session.SessionId), cancellationToken: cancellationToken);

            _logger.LogInformation("Created chat session: {SessionId}", session.SessionId);
            return session;
        }

        public async Task<ChatSession?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            try
            {
                // Session doc has id = sessionId and type = "session"
                var response = await _container.ReadItemAsync<ChatSession>(
                    sessionId, new PartitionKey(sessionId), cancellationToken: cancellationToken);
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<List<ChatSession>> GetAllSessionsAsync(CancellationToken cancellationToken = default)
        {
            var query = _container.GetItemLinqQueryable<ChatSession>()
                .Where(x => x.Type == "session")
                .OrderByDescending(x => x.LastMessageAt);

            var sessions = new List<ChatSession>();
            using var iterator = query.ToFeedIterator();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                sessions.AddRange(response);
            }

            return sessions;
        }

        public async Task SaveMessageAsync(ChatMessage message, CancellationToken cancellationToken = default)
        {
            await _container.CreateItemAsync(
                message,
                new PartitionKey(message.SessionId),
                cancellationToken: cancellationToken);

            _logger.LogDebug("Saved {Role} message to session {SessionId}", message.Role, message.SessionId);
        }

        public async Task<List<ChatMessage>> GetRecentMessagesAsync(
            string sessionId, int count = 10, CancellationToken cancellationToken = default)
        {
            var query = _container.GetItemLinqQueryable<ChatMessage>()
                .Where(x => x.SessionId == sessionId && x.Type == "message")
                .OrderByDescending(x => x.Timestamp)
                .Take(count);

            var messages = new List<ChatMessage>();
            using var iterator = query.ToFeedIterator();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                messages.AddRange(response);
            }

            // Reverse so oldest first (chronological order for the LLM)
            messages.Reverse();
            return messages;
        }

        public async Task UpdateSessionAsync(ChatSession session, CancellationToken cancellationToken = default)
        {
            session.LastMessageAt = DateTime.UtcNow;
            await _container.UpsertItemAsync(
                session,
                new PartitionKey(session.SessionId),
                cancellationToken: cancellationToken);
        }
    }
}
*/