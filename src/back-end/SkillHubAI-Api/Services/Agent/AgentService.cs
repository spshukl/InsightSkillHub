using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using SkillHubAI_Api.Configurations.settings;

//using SkillHubAI_Api.Configurations.Settings;//
using SkillHubAI_Api.Models;
using SkillHubAI_Api.Services.Chat;
using System.ClientModel;
using System.Collections.Concurrent;
using AIChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace SkillHubAI_Api.Services.Agent
{
    public sealed class AgentService : IAgentService
    {
        private AIAgent? _agent;
        private readonly IKnowledgeRetriever _retriever;
        private readonly Container _cosmosContainer;
        private readonly ILogger<AgentService> _logger;
        private readonly CosmosDbChatHistoryProvider _historyProvider;
        private readonly ConcurrentDictionary<string, AgentSession> _sessions = new();
        private List<ChatCitation> _lastCitations = new();
        private readonly IChatClient _chatClient;

        private const string SystemInstructions = """
            You are SkillHubAI, an intelligent assistant that answers questions based on the provided knowledge base.

            RULES:
            1. Answer ONLY based on the provided context from the search_knowledge tool.
            2. ALWAYS call the search_knowledge tool before answering.
            3. Be concise and accurate.
            4. When you use information from the context, reference it naturally.
            5. Do not make up information or hallucinate facts.
            6. If the context doesn't contain relevant information, say "I don't have enough information to answer that question."
            7. Use markdown formatting for better readability.
            """;

        public AgentService(
            IChatClient chatClient,
            IKnowledgeRetriever retriever,
            CosmosClient cosmosClient,
            IOptions<CosmosDbSettings> cosmosSettings,
            CosmosDbChatHistoryProvider historyProvider,
            ILogger<AgentService> logger)
        {
            _retriever = retriever;
            _logger = logger;
            _historyProvider = historyProvider;
            _chatClient = chatClient;
            var db = cosmosClient.GetDatabase(cosmosSettings.Value.DatabaseName);
            _cosmosContainer = db.GetContainer("ChatHistory");


        }
        private AIAgent EnsureAgent()
        {
            if (_agent is not null)
                return _agent;

            _logger.LogInformation("Creating agent...");

            _agent = new ChatClientAgent(
                chatClient: _chatClient,
                options: new ChatClientAgentOptions
                {
                    Name = "SkillHubAI",
                    ChatOptions = new()
                    {
                        Instructions = SystemInstructions
                    },
                    ChatHistoryProvider = _historyProvider,
                    AIContextProviders = new List<AIContextProvider>
                    {
                        new TextSearchProvider(SearchAdapter, new TextSearchProviderOptions
                        {
                            SearchTime = TextSearchProviderOptions.TextSearchBehavior.BeforeAIInvoke,
                            RecentMessageMemoryLimit = 6
                        })
                    }
                });

            return _agent;
        }

        private async Task<IEnumerable<TextSearchProvider.TextSearchResult>> SearchAdapter(
          string query, CancellationToken cancellationToken)
        {
            _logger.LogInformation("RAG Search — Query: {Query}", query);

            _lastCitations = await _retriever.RetrieveAsync(query, topK: 5, cancellationToken);

            return _lastCitations.Select(c => new TextSearchProvider.TextSearchResult
            {
                Text = c.ChunkContent,
                SourceName = c.SourceFileName,
                SourceLink = c.SourceFileId
            });
        }


        public async Task<(string SessionId, AgentSessionInfo Info)> CreateSessionAsync(
            CancellationToken cancellationToken = default)
        {
            var sessionId = Guid.NewGuid().ToString();
            var session = await EnsureAgent().CreateSessionAsync(cancellationToken);

            _historyProvider.SetSessionId(session, sessionId);

            _sessions[sessionId] = session;
            var sessionDoc = new ChatSession
            {
                Id = sessionId,
                SessionId = sessionId,
                Title = "New Chat",
                CreatedAt = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow
            };

            await _cosmosContainer.CreateItemAsync(
                sessionDoc,
                new PartitionKey(sessionId),
                cancellationToken: cancellationToken);

            _logger.LogInformation("Created agent session: {SessionId}", sessionId);

            return (sessionId, new AgentSessionInfo
            {
                SessionId = sessionId,
                Title = "New Chat",
                CreatedAt = sessionDoc.CreatedAt
            });
        }

        public async Task<string> ChatAsync(
            string sessionId,
            string userMessage,
            CancellationToken cancellationToken = default)
        {
            // Get or restore session
            if (!_sessions.TryGetValue(sessionId, out var session))
            {
                session = await RestoreSessionAsync(sessionId, cancellationToken);

                if (session is null)
                    throw new InvalidOperationException($"Session '{sessionId}' not found");
            }

            _logger.LogInformation("Agent chat — Session: {SessionId}, Message: {Message}",
                sessionId, userMessage);

            var messages = new List<AIChatMessage>
            {
                new(ChatRole.User, userMessage)
            };

            var response = await _agent!.RunAsync(messages, session, cancellationToken: cancellationToken);

            // Update session metadata in Cosmos
            await UpdateSessionMetadataAsync(sessionId, userMessage, cancellationToken);

            return response.Text;
        }

        public Task<List<ChatCitation>> GetLastCitationsAsync()
        {
            return Task.FromResult(_lastCitations);
        }

        public async Task<AgentSessionInfo?> GetSessionInfoAsync(
            string sessionId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _cosmosContainer.ReadItemAsync<ChatSession>(
                    sessionId,
                    new PartitionKey(sessionId),
                    cancellationToken: cancellationToken);

                var doc = response.Resource;
                return new AgentSessionInfo
                {
                    SessionId = doc.SessionId,
                    Title = doc.Title,
                    CreatedAt = doc.CreatedAt,
                    MessageCount = doc.MessageCount
                };
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

       
        private async Task<AgentSession?> RestoreSessionAsync(
            string sessionId,
            CancellationToken cancellationToken)
        {
            try
            {
                
                await _cosmosContainer.ReadItemAsync<ChatSession>(
                    sessionId,
                    new PartitionKey(sessionId),
                    cancellationToken: cancellationToken);

              
                var session = await EnsureAgent().CreateSessionAsync(cancellationToken);

                _historyProvider.SetSessionId(session, sessionId);
                _sessions[sessionId] = session;

                _logger.LogInformation("Restored session: {SessionId}", sessionId);
                return session;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        private async Task UpdateSessionMetadataAsync(
            string sessionId,
            string userMessage,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await _cosmosContainer.ReadItemAsync<ChatSession>(
                    sessionId,
                    new PartitionKey(sessionId),
                    cancellationToken: cancellationToken);

                var doc = response.Resource;

                if (doc.MessageCount == 0)
                {
                    doc.Title = userMessage.Length > 50
                        ? userMessage[..50] + "..."
                        : userMessage;
                }

                doc.MessageCount += 2;
                doc.LastMessageAt = DateTime.UtcNow;

                await _cosmosContainer.UpsertItemAsync(
                    doc,
                    new PartitionKey(sessionId),
                    cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update session metadata: {SessionId}", sessionId);
            }
        }
    }
}
