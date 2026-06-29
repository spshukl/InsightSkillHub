using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents.Indexes;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DataIngestion;
using Microsoft.Extensions.DataIngestion.Chunkers;
using Microsoft.Extensions.Options;
using Microsoft.ML.Tokenizers;
using Microsoft.SemanticKernel.Connectors.AzureAISearch;
using SkillHubAI_Api.Configurations.settings;
using SkillHubAI_Api.Models;
using SkillHubAI_Api.Services.Queue;
using SkillHubAI_Api.Services.Status;
using SkillHubAI_Api.Services.Storage;
using System.ClientModel;

namespace SkillHubAI_Api.Services.Ingestion
{
    public sealed class DataIngestionService : BackgroundService
    {
        private readonly IIngestionQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DataIngestionService> _logger;
        private readonly AzureOpenAISettings _openAISettings;
        private readonly AzureAISearchSettings _searchSettings;
        private readonly IngestionSettings _ingestionSettings;
        private readonly AzureOpenAIClient _openAIClient;
        private readonly IChatClient _chatClient;
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
        private readonly SearchIndexClient _searchIndexClient;
       
        //mime type map krne k liye
        private static readonly Dictionary<string, string> MediaTypeMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { ".pdf",  "application/pdf" },
            { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
            { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
            { ".txt",  "text/plain" },
            { ".md",   "text/markdown" },
            { ".html", "text/html" },
            { ".htm",  "text/html" },
            { ".csv",  "text/csv" },
            { ".json", "application/json" },
            { ".xml",  "application/xml" }
        };

        public DataIngestionService(
            IIngestionQueue queue,
            IServiceScopeFactory scopeFactory,
            IOptions<AzureOpenAISettings> openAISettings,
            IOptions<AzureAISearchSettings> searchSettings,
            IOptions<IngestionSettings> ingestionSettings,
             AzureOpenAIClient openAIClient,                                   
        IChatClient chatClient,                                             
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,   
        SearchIndexClient searchIndexClient,

            ILogger<DataIngestionService> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
            _openAISettings = openAISettings.Value;
            _searchSettings = searchSettings.Value;
            _ingestionSettings = ingestionSettings.Value;
            _openAIClient = openAIClient;               
            _chatClient = chatClient;                      
            _embeddingGenerator = embeddingGenerator;      
            _searchIndexClient = searchIndexClient;

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "DataIngestionService started — Index: {Index}, Strategy: {Strategy}, MaxTokens: {MaxTokens}",
                _searchSettings.IndexName,
                _ingestionSettings.ChunkingStrategy,
                _ingestionSettings.MaxTokensPerChunk);

            await foreach (var req in _queue.DequeueAllAsync(stoppingToken))
            {
                try
                {
                    _logger.LogInformation(
                        "Processing job — FileId: {FileId}, File: {FileName}",
                        req.FileId, req.FileName);

                    await ProcessJobAsync(req, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("DataIngestionService shutting down gracefully");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Unhandled error — FileId: {FileId}", req.FileId);

                    await SafeUpdateStatusAsync(
                        req.FileId, IngestionStatus.Failed,
                        $"Unexpected error: {ex.Message}", stoppingToken);
                }
            }

            _logger.LogInformation("DataIngestionService stopped");
        }

        private async Task ProcessJobAsync(IngestionRequest req, CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var statusHandler = scope.ServiceProvider.GetRequiredService<IIngestionStatusHandler>();
            var storageService = scope.ServiceProvider.GetRequiredService<IAzureStorageService>();
            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();

         
            
            await statusHandler.UpdateStatusAsync(
                req.FileId, IngestionStatus.Extracting,
                "Downloading document from blob storage", cancellationToken);

            _logger.LogInformation("[{FileId}] Downloading blob: {BlobUri}", req.FileId, req.BlobUri);

            using var blobStream = await storageService.DownloadBlobAsync(req.BlobUri, cancellationToken);

            var tempDir = Path.Combine(Path.GetTempPath(), "skillhubai", req.FileId);
            Directory.CreateDirectory(tempDir);
            var tempFilePath = Path.Combine(tempDir, req.FileName);

            try
            {
                // Save blob to temp file
                await using (var fileStream = File.Create(tempFilePath))
                {
                    await blobStream.CopyToAsync(fileStream, cancellationToken);
                }

                _logger.LogInformation("[{FileId}] Saved to temp: {Path}", req.FileId, tempFilePath);

          
                IngestionDocumentReader reader = new MarkItDownReader();

                // Chunk ho rha hai
                await statusHandler.UpdateStatusAsync(
                    req.FileId, IngestionStatus.Chunking,
                    $"Splitting with {_ingestionSettings.ChunkingStrategy} strategy", cancellationToken);
                //code fat rha hei yha pe
                /* var tokenizer = TiktokenTokenizer.CreateForModel("gpt-5.4");*/
                var tokenizer = TiktokenTokenizer.CreateForEncoding("cl100k_base");

                var chunkerOptions = new IngestionChunkerOptions(tokenizer)
                {
                    MaxTokensPerChunk = _ingestionSettings.MaxTokensPerChunk,
                    OverlapTokens = _ingestionSettings.OverlapTokens
                };

                IngestionChunker<string> chunker = _ingestionSettings.ChunkingStrategy switch
                {
                    "Section" => new SectionChunker(chunkerOptions),
                    _ => new HeaderChunker(chunkerOptions)
                };

                // Enrichers
               

              

             /*   var enricherOptions = new EnricherOptions(_chatClient)
                {
                    LoggerFactory = loggerFactory,
                    BatchSize = 1
                };*/
              /*  var summaryEnricher = new SummaryEnricher(enricherOptions, maxWordCount: 100);
                var keywordEnricher = new KeywordEnricher(
                    enricherOptions,
                    predefinedKeywords: ReadOnlySpan<string>.Empty,
                    maxKeywords: 5,
                    confidenceThreshold: 0.7);*/

                // Embedding generator
                await statusHandler.UpdateStatusAsync(
                    req.FileId, IngestionStatus.Embedding,
                    "Generating embeddings via Azure OpenAI", cancellationToken);

                using var vectorStore = new AzureAISearchVectorStore(
                    _searchIndexClient,
                    new AzureAISearchVectorStoreOptions
                    {
                        EmbeddingGenerator = _embeddingGenerator
                    });

                using var writer = new VectorStoreWriter<string>(
                    vectorStore,
                    _openAISettings.EmbeddingDimensions,
                    new VectorStoreWriterOptions
                    {
                        CollectionName = _searchSettings.IndexName
                    });

                // ─── ASSEMBLE & RUN PIPELINE ───
                await statusHandler.UpdateStatusAsync(
                    req.FileId, IngestionStatus.Storing,
                    "Executing ingestion pipeline", cancellationToken);

                _logger.LogInformation("[{FileId}] PIPELINE — Executing", req.FileId);

                using var pipeline = new IngestionPipeline<string>(
                    reader, chunker, writer, loggerFactory: loggerFactory)
                {
                   // ChunkProcessors = { summaryEnricher, keywordEnricher }
                };

              
                var tempFile = new FileInfo(tempFilePath);
                int totalDocs = 0;
                bool succeeded = false;
                string? lastError = null;

                await foreach (var result in pipeline.ProcessAsync(new[] { tempFile }, cancellationToken))
                {
                    if (result.Succeeded)
                    {
                        totalDocs++;
                        succeeded = true;
                        _logger.LogInformation(
                            "[{FileId}] PIPELINE ✓ — Document: {DocId}",
                            req.FileId, result.DocumentId);
                    }
                    else
                    {
                        lastError = result.Exception?.Message;
                        _logger.LogError(result.Exception,
                            "[{FileId}] PIPELINE ✗ — Document: {DocId}",
                            req.FileId, result.DocumentId);
                    }
                }

                
                if (succeeded)
                {
                    var metadata = await statusHandler.GetIngestionStatusAsync(req.FileId, cancellationToken);
                    if (metadata is not null)
                    {
                        metadata.Status = IngestionStatus.Completed;
                        metadata.CompletedAt = DateTime.UtcNow;
                        metadata.StatusMessage =
                            $"Ingested into index '{_searchSettings.IndexName}'";
                        await statusHandler.UpdateIngestionStatusAsync(metadata, cancellationToken);
                    }

                    _logger.LogInformation(
                        "[{FileId}] ✅ COMPLETE → index '{Index}'",
                        req.FileId, _searchSettings.IndexName);
                }
                else
                {
                    await statusHandler.UpdateStatusAsync(
                        req.FileId, IngestionStatus.Failed,
                        $"Pipeline failed: {lastError}", cancellationToken);

                    _logger.LogError("[{FileId}] ❌ FAILED: {Error}", req.FileId, lastError);
                }
            }
            finally
            {
              
                try
                {
                    if (Directory.Exists(tempDir))
                        Directory.Delete(tempDir, recursive: true);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "[{FileId}] Failed to clean temp dir: {Dir}", req.FileId, tempDir);
                }
            }
        }

        private static string GetMediaType(string fileName)
        {
            var extension = Path.GetExtension(fileName);

            if (!string.IsNullOrEmpty(extension) && MediaTypeMap.TryGetValue(extension, out var mediaType))
            {
                return mediaType;
            }
            return "application/octet-stream";
        }
        private async Task SafeUpdateStatusAsync(
            string fileId, IngestionStatus status, string message,
            CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<IIngestionStatusHandler>();
                await handler.UpdateStatusAsync(fileId, status, message, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to update status to {Status} for FileId: {FileId}", status, fileId);
            }
        }
    }
}

