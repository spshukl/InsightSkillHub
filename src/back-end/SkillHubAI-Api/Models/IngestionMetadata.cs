using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SkillHubAI_Api
{

    public class IngestionMetadata
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("fileId")]
        public string FileId { get; set; } = string.Empty;

        [JsonProperty("fileName")]
        public string FileName { get; set; } = string.Empty;

        [JsonProperty("blobName")]
        public string BlobName { get; set; } = string.Empty;

        [JsonProperty("containerName")]
        public string ContainerName { get; set; } = string.Empty;

        [JsonProperty("blobUri")]
        public string BlobUri { get; set; } = string.Empty;

        [JsonProperty("status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public IngestionStatus Status { get; set; } = IngestionStatus.Uploaded;

        [JsonProperty("statusMessage")]
        public string? StatusMessage { get; set; }

        [JsonProperty("chunkCount")]
        public int ChunkCount { get; set; }

        [JsonProperty("fileSizeBytes")]
        public long FileSizeBytes { get; set; }

        [JsonProperty("contentType")]
        public string? ContentType { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [JsonProperty("completedAt")]
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Cosmos DB partition key — using FileId for point reads.
        /// </summary>
        [JsonProperty("partitionKey")]
        public string PartitionKey => FileId;



    }
}
