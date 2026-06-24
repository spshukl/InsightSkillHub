/*using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using SkillHubAI_Api.Configurations.settings;

//using SkillHubAI_Api.Configurations.Settings;
using SkillHubAI_Api.Models;
using System.ClientModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace SkillHubAI_Api.Services.Chat
{
    public sealed class RagAgent : IRagAgent
    {
        private readonly IKnowledgeRetriever _retriever;
        private readonly AzureOpenAISettings _openAISettings;
        private readonly ILogger<RagAgent> _logger;
        private List<ChatCitation> _lastCitations = new();
        private readonly IChatClient _agentClient;
       // private readonly AzureOpenAIClient azureOpenAI;

        private const string SystemPrompt = """
            You are SkillHubAI, an intelligent assistant that answers questions based on the provided knowledge base.

            RULES:
            1. Answer ONLY based on the provided context. If the context doesn't contain relevant information, say "I don't have enough information to answer that question."
            2. Be concise and accurate.
            3. When you use information from the context, reference it naturally (e.g., "According to the document...").
            4. Do not make up information or hallucinate facts.
            5. If the user asks something unrelated to the knowledge base, politely redirect them.
            6. Use markdown formatting for better readability.
            """;

        public RagAgent(
            IKnowledgeRetriever retriever,
            IOptions<AzureOpenAISettings> openAISettings,
            IChatClient agentClient,
           
            ILogger<RagAgent> logger)
        {
            _retriever = retriever;
            _openAISettings = openAISettings.Value;
            _logger = logger;
            _agentClient = agentClient;
        }

        public async IAsyncEnumerable<string> ChatStreamAsync(
         string userMessage,
         List<Models.ChatMessage> chatHistory,
         [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // ─── RETRIEVE ───
            _logger.LogInformation("RAG Agent — Retrieving knowledge for: {Query}", userMessage);

            _lastCitations = await _retriever.RetrieveAsync(userMessage, topK: 5, cancellationToken);

            _logger.LogInformation("RAG Agent — Retrieved {Count} citations", _lastCitations.Count);

            // ─── BUILD GROUNDED PROMPT ───
            var contextBuilder = new StringBuilder();

            if (_lastCitations.Count > 0)
            {
                contextBuilder.AppendLine("## Retrieved Knowledge Context:");
                contextBuilder.AppendLine();

                for (int i = 0; i < _lastCitations.Count; i++)
                {
                    var citation = _lastCitations[i];
                    contextBuilder.AppendLine($"### Source {i + 1} (File: {citation.SourceFileName}, Relevance: {citation.RelevanceScore:F2})");
                    contextBuilder.AppendLine(citation.ChunkContent);
                    contextBuilder.AppendLine();
                }
            }
            else
            {
                contextBuilder.AppendLine("No relevant documents were found in the knowledge base.");
            }

                       
                   
            var messages = new List<Microsoft.Extensions.AI.ChatMessage>();
            messages.Add(new Microsoft.Extensions.AI.ChatMessage(ChatRole.System, SystemPrompt));

            // Add chat history
            foreach (var historyMsg in chatHistory)
            {
                var role = historyMsg.Role == "user" ? ChatRole.User : ChatRole.Assistant;
                messages.Add(new Microsoft.Extensions.AI.ChatMessage(role, historyMsg.Content));
            }

                        // Add grounded user message
                        var groundedMessage = $"""
                {contextBuilder}

                ## User Question:
                {userMessage}
                """;

            messages.Add(new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, groundedMessage));


            // ─── STREAM RESPONSE ───
            _logger.LogInformation("RAG Agent — Streaming response from {Model}", _openAISettings.ChatDeployment);

           *//* var openAIClient = new AzureOpenAIClient(
                new Uri(_openAISettings.Endpoint),
                new ApiKeyCredential(_openAISettings.ApiKey));

            IChatClient chatClient = openAIClient
                .GetChatClient(_openAISettings.ChatDeployment)
                .AsIChatClient();*//*

            await foreach (var update in _agentClient.GetStreamingResponseAsync(
                messages, cancellationToken: cancellationToken))
            {
                if (update.Text is not null)
                {
                    yield return update.Text;
                }
            }
        }

        public List<ChatCitation> GetLastCitations() => _lastCitations;

        public async Task<string> ChatAsync(
     string userMessage,
     List<Models.ChatMessage> chatHistory,
     CancellationToken cancellationToken = default)
        {
           
            _logger.LogInformation("RAG Agent — Retrieving knowledge for: {Query}", userMessage);

            _lastCitations = await _retriever.RetrieveAsync(userMessage, topK: 5, cancellationToken);

            _logger.LogInformation("RAG Agent — Retrieved {Count} citations", _lastCitations.Count);

          
            var contextBuilder = new StringBuilder();

            if (_lastCitations.Count > 0)
            {
                contextBuilder.AppendLine("## Retrieved Knowledge Context:");
                contextBuilder.AppendLine();

                for (int i = 0; i < _lastCitations.Count; i++)
                {
                    var citation = _lastCitations[i];
                    contextBuilder.AppendLine($"### Source {i + 1} (File: {citation.SourceFileName}, Relevance: {citation.RelevanceScore:F2})");
                    contextBuilder.AppendLine(citation.ChunkContent);
                    contextBuilder.AppendLine();
                }
            }
            else
            {
                contextBuilder.AppendLine("No relevant documents were found in the knowledge base.");
            }

           
            var messages = new List<Microsoft.Extensions.AI.ChatMessage>();
            messages.Add(new Microsoft.Extensions.AI.ChatMessage(ChatRole.System, SystemPrompt));

            foreach (var historyMsg in chatHistory)
            {
                var role = historyMsg.Role == "user" ? ChatRole.User : ChatRole.Assistant;
                messages.Add(new Microsoft.Extensions.AI.ChatMessage(role, historyMsg.Content));
            }

            var groundedMessage = $"""
        {contextBuilder}

        ## User Question:
        {userMessage}
        """;

            messages.Add(new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, groundedMessage));

           
            _logger.LogInformation("RAG Agent — Getting response from {Model}", _openAISettings.ChatDeployment);

            var response = await _agentClient.GetResponseAsync(messages, cancellationToken: cancellationToken);

            return response.Text ?? string.Empty;
        }

    }
}
*/