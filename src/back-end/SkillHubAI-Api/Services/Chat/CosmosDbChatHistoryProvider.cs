//using Microsoft.Agents.AI;

using Microsoft.Agents.AI;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using SkillHubAI_Api.Configurations.settings;
using System.Text.Json.Serialization;

namespace SkillHubAI_Api.Services.Chat
{
    public sealed class CosmosDbChatHistoryProvider : ChatHistoryProvider
    {
        private readonly Container _container;
        private readonly ILogger<CosmosDbChatHistoryProvider> _logger;
        private readonly int _maxMessages;
        private readonly ProviderSessionState<CosmosSessionState> _sessionState;

        public CosmosDbChatHistoryProvider(
            CosmosClient cosmosClient,
            IOptions<CosmosDbSettings> settings,
            ILogger<CosmosDbChatHistoryProvider> logger,
            int maxMessages = 20)
        {
            var db = cosmosClient.GetDatabase(settings.Value.DatabaseName);
            _container = db.GetContainer("ChatHistory");
            _logger = logger;
            _maxMessages = maxMessages;

            _sessionState = new ProviderSessionState<CosmosSessionState>(
                stateInitializer: _ => new CosmosSessionState
                {
                    SessionId = Guid.NewGuid().ToString()
                },
                stateKey: GetType().Name);
        }

        protected override async ValueTask<IEnumerable<ChatMessage>> ProvideChatHistoryAsync(
            InvokingContext context,
            CancellationToken cancellationToken = default)
        {
            var state = _sessionState.GetOrInitializeState(context.Session);

            _logger.LogDebug("Loading history for session: {SessionId}", state.SessionId);

            try
            {
                var query = _container.GetItemLinqQueryable<CosmosMessageDocument>()
                    .Where(x => x.SessionId == state.SessionId && x.Type == "message")
                    .OrderByDescending(x => x.Timestamp)
                    .Take(_maxMessages);

                var messages = new List<ChatMessage>();
                using var iterator = query.ToFeedIterator();

                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync(cancellationToken);
                    foreach (var doc in response)
                    {
                        var role = doc.Role == "user" ? ChatRole.User : ChatRole.Assistant;
                        messages.Add(new ChatMessage(role, doc.Content));
                    }
                }

                messages.Reverse();

                _logger.LogDebug("Loaded {Count} messages", messages.Count);
                return messages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load history for {SessionId}", state.SessionId);
                return [];
            }
        }

        protected override async ValueTask StoreChatHistoryAsync(
            InvokedContext context,
            CancellationToken cancellationToken = default)
        {
            var state = _sessionState.GetOrInitializeState(context.Session);

            var allNewMessages = context.RequestMessages
                .Concat(context.ResponseMessages ?? []);

            foreach (var message in allNewMessages)
            {
                var doc = new CosmosMessageDocument
                {
                    Id = Guid.NewGuid().ToString(),
                    SessionId = state.SessionId,
                    Role = message.Role == ChatRole.User ? "user" : "assistant",
                    Content = message.Text ?? string.Empty,
                    Timestamp = DateTime.UtcNow
                };

                try
                {
                    await _container.CreateItemAsync(
                        doc,
                        new PartitionKey(state.SessionId),
                        cancellationToken: cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to store message for {SessionId}", state.SessionId);
                }
            }
        }

        public void SetSessionId(AgentSession session, string sessionId)
        {
            var state = new CosmosSessionState { SessionId = sessionId };
            _sessionState.SaveState(session, state);
        }

      
        public string GetSessionId(AgentSession session)
        {
            return _sessionState.GetOrInitializeState(session).SessionId;
        }

        public sealed class CosmosSessionState
        {
            [JsonPropertyName("sessionId")]
            public string SessionId { get; set; } = string.Empty;
        }

        private sealed class CosmosMessageDocument
        {
            [JsonPropertyName("id")]
            public string Id { get; set; } = string.Empty;

            [JsonPropertyName("sessionId")]
            public string SessionId { get; set; } = string.Empty;

            [JsonPropertyName("type")]
            public string Type { get; set; } = "message";

            [JsonPropertyName("role")]
            public string Role { get; set; } = string.Empty;

            [JsonPropertyName("content")]
            public string Content { get; set; } = string.Empty;

            [JsonPropertyName("timestamp")]
            public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        }
    }
}

