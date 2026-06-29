namespace SkillHubAI_Api.Services.Status
{
    public interface IIngestionStatusHandler
    {
        Task<IngestionMetadata?> GetIngestionStatusAsync(
            string fileId,
            CancellationToken cancellationToken = default);

        Task CreateIngestionStatusAsync(
            IngestionMetadata metadata,
            CancellationToken cancellationToken = default);

        Task UpdateIngestionStatusAsync(
            IngestionMetadata metadata,
            CancellationToken cancellationToken = default);

        Task UpdateStatusAsync(
            string fileId,
            IngestionStatus status,
            string? message = null,
            CancellationToken cancellationToken = default);
    }
}
