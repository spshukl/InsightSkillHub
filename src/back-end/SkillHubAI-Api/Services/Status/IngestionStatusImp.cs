using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using SkillHubAI_Api.Configurations.settings;
using SkillHubAI_Api.Controllers.DataSource;

namespace SkillHubAI_Api.Services.Status
{

    public sealed class IngestionStatusImp : IIngestionStatusHandler
    {
        private readonly Container _container;
        private readonly ILogger<IngestionStatusImp> _logger;

        public IngestionStatusImp(
            CosmosClient cosmosClient,
            IOptions<CosmosDbSettings> settings,
            ILogger<IngestionStatusImp> logger)
        {
            _logger = logger;
            var db = cosmosClient.GetDatabase(settings.Value.DatabaseName);
            _container = db.GetContainer(settings.Value.ContainerName);
        }

        public async Task CreateIngestionStatusAsync(
            IngestionMetadata metadata,
            CancellationToken cancellationToken = default)
        {
            // Use FileId as both the document id and partition key
            metadata.Id = metadata.FileId;

            await _container.CreateItemAsync(
                metadata,
                new PartitionKey(metadata.PartitionKey),
                cancellationToken: cancellationToken);

            _logger.LogDebug("Created ingestion status — FileId: {FileId}", metadata.FileId);
        }

        public async Task<IngestionMetadata?> GetIngestionStatusAsync(
            string fileId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _container.ReadItemAsync<IngestionMetadata>(
                    fileId,
                    new PartitionKey(fileId),
                    cancellationToken: cancellationToken);

                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogDebug("Ingestion status not found — FileId: {FileId}", fileId);
                return null;
            }
        }

        public async Task UpdateIngestionStatusAsync(
            IngestionMetadata metadata,
            CancellationToken cancellationToken = default)
        {
            metadata.UpdatedAt = DateTime.UtcNow;

            await _container.UpsertItemAsync(
                metadata,
                new PartitionKey(metadata.PartitionKey),
                cancellationToken: cancellationToken);

            _logger.LogDebug(
                "Updated ingestion status — FileId: {FileId}, Status: {Status}",
                metadata.FileId, metadata.Status);
        }

        public async Task UpdateStatusAsync(
            string fileId,
            IngestionStatus status,
            string? message = null,
            CancellationToken cancellationToken = default)
        {
            var metadata = await GetIngestionStatusAsync(fileId, cancellationToken);

            if (metadata is null)
            {
                _logger.LogWarning(
                    "Cannot update status — document not found. FileId: {FileId}", fileId);
                return;
            }

            metadata.Status = status;
            metadata.StatusMessage = message;
            metadata.UpdatedAt = DateTime.UtcNow;

            if (status is IngestionStatus.Completed or IngestionStatus.Failed)
            {
                metadata.CompletedAt = DateTime.UtcNow;
            }

            await UpdateIngestionStatusAsync(metadata, cancellationToken);
        }
    }
}
