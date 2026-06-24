namespace SkillHubAI_Api.Models
{
    public sealed class IngestionJob
    {
        public required string FileId { get; init; }
        public required string BlobUri { get; init; }
        public required string FileName { get; init; }
        public required string BlobName { get; init; }
        public required string ContainerName { get; init; }
        public DateTime QueuedAt { get; init; } = DateTime.UtcNow;
    }
}
