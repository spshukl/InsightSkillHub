
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using OpenAI.Containers;
using SkillHubAI_Api.Configurations.settings;
using SkillHubAI_Api.Controllers.DataSource;
using SkillHubAI_Api.Models;
using SkillHubAI_Api.Services.Queue;
using SkillHubAI_Api.Services.Status;

namespace SkillHubAI_Api.Services.Storage
{
    public sealed class AzureStorageServiceImp : IAzureStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IIngestionStatusHandler _statusHandler;
        private readonly IIngestionQueue _ingestionQueue;
        private readonly AzureBlobStorageSettings _settings;
        private readonly ILogger<AzureStorageServiceImp> _logger;

        public AzureStorageServiceImp(
            BlobServiceClient blobServiceClient,
            IIngestionStatusHandler statusHandler,
            IIngestionQueue ingestionQueue,
            IOptions<AzureBlobStorageSettings> settings,
            ILogger<AzureStorageServiceImp> logger)
        {
            _blobServiceClient = blobServiceClient;
            _statusHandler = statusHandler;
            _ingestionQueue = ingestionQueue;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<IngestionMetadata> UploadFileAsync(
            IFormFile file,
            string fileId,
            CancellationToken cancellationToken = default)
        {
            // Generate a collision-safe blob name
            var safeFileName = Path.GetFileNameWithoutExtension(file.FileName);
            var extension = Path.GetExtension(file.FileName);
            var blobName = $"{safeFileName}-{fileId}{extension}";

            // Get or create the container
            var containerClient = _blobServiceClient.GetBlobContainerClient(_settings.ContainerName);
            await containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

            var blobClient = containerClient.GetBlobClient(blobName);

            // Upload the file stream
            await using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true, cancellationToken);
            }

            _logger.LogInformation(
                "Blob uploaded — Container: {Container}, Blob: {Blob}, Size: {Size} bytes",
                _settings.ContainerName, blobName, file.Length);

            // Create metadata record in Cosmos DB
            var metadata = new IngestionMetadata
            {
                FileId = fileId,
                FileName = file.FileName,
                BlobName = blobName,
                ContainerName = _settings.ContainerName,
                BlobUri = blobClient.Uri.AbsoluteUri,
                FileSizeBytes = file.Length,
                ContentType = file.ContentType,
                Status = IngestionStatus.Uploaded,
                CreatedAt = DateTime.UtcNow
            };

            await _statusHandler.CreateIngestionStatusAsync(metadata, cancellationToken);

            // Enqueue for background processing — returns immediately
            var job = new IngestionJob
            {
                FileId = fileId,
                BlobUri = blobClient.Uri.AbsoluteUri,
                FileName = file.FileName,
                BlobName = blobName,
                ContainerName = _settings.ContainerName
            };

            await _ingestionQueue.EnqueueAsync(job, cancellationToken);

            // Update status to Queued
            await _statusHandler.UpdateStatusAsync(
                fileId, IngestionStatus.Queued,
                "Job queued for background ingestion", cancellationToken);

            _logger.LogInformation("Ingestion job queued — FileId: {FileId}", fileId);

            return metadata;
        }

        public async Task<Stream> DownloadBlobAsync(
            string blobUri,
            CancellationToken cancellationToken = default)
        {
            // Parse the URI to extract container and blob name,
            // then use the authenticated BlobServiceClient
            var uri = new Uri(blobUri);
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length < 2)
            {
                throw new ArgumentException($"Invalid blob URI format: {blobUri}", nameof(blobUri));
            }

            var containerName = segments[0];
            var blobName = string.Join("/", segments.Skip(1));

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);
            return response.Value.Content;
        }
    }
}
