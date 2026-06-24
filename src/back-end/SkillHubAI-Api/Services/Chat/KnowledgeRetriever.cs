using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using SkillHubAI_Api.Configurations.settings;
using SkillHubAI_Api.Models;
using System.ClientModel;

namespace SkillHubAI_Api.Services.Chat
{
    public sealed class KnowledgeRetriever : IKnowledgeRetriever
    {
        private readonly SearchClient _searchClient;
     
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
        private readonly ILogger<KnowledgeRetriever> _logger;

        public KnowledgeRetriever(
            IOptions<AzureAISearchSettings> searchSettings,
            IOptions<AzureOpenAISettings> openAISettings,
            AzureOpenAIClient openAIClient,
            SearchClient searchClient,
            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
            ILogger<KnowledgeRetriever> logger)
        {
            var settings = searchSettings.Value;
            _searchClient = searchClient;
           // _openAISettings = openAISettings.Value;
            _embeddingGenerator= embeddingGenerator;
          //  _openAIClient = openAIClient;

            _logger = logger;
        }

        public async Task<List<ChatCitation>> RetrieveAsync(
            string query,
            int topK = 5,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Retrieving knowledge for query: {Query}", query);


            var embeddingResponse = await _embeddingGenerator.GenerateAsync(query, cancellationToken: cancellationToken);
            var queryVector = embeddingResponse.Vector;

            // Hybrid search: vector + keyword
            var searchOptions = new SearchOptions
            {
                Size = topK,
                Select = { "content", "documentid", "context" },
                VectorSearch = new()
                {
                    Queries =
                    {
                        new VectorizedQuery(queryVector)
                        {
                            KNearestNeighborsCount = topK,
                            Fields = { "embedding" }
                        }
                    }
                }
            };

            var response = await _searchClient.SearchAsync<SearchDocument>(
                query, // keyword search component
                searchOptions,
                cancellationToken);

            var citations = new List<ChatCitation>();

            await foreach (var result in response.Value.GetResultsAsync())
            {
                var doc = result.Document;

                // VectorStoreWriter uses these field names
                doc.TryGetValue("content", out var contentObj);
                doc.TryGetValue("documentid", out var docIdObj);
                doc.TryGetValue("context", out var contextObj);

                var content = contentObj?.ToString() ?? string.Empty;
                var documentId = docIdObj?.ToString() ?? string.Empty;

                citations.Add(new ChatCitation
                {
                    SourceFileId = documentId,
                    SourceFileName = documentId, // We can look up the real name from Cosmos if needed
                    ChunkContent = content,
                    RelevanceScore = result.Score ?? 0
                });
            }

            _logger.LogInformation("Retrieved {Count} citations", citations.Count);
            return citations;
        }
    }
}


