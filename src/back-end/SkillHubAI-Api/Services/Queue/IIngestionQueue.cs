using SkillHubAI_Api.Models;

namespace SkillHubAI_Api.Services.Queue
{
    public interface IIngestionQueue
    {
        /// <summary>Enqueue a job for background processing.</summary>
        ValueTask EnqueueAsync(IngestionJob job, CancellationToken cancellationToken = default);

        /// <summary>Continuously dequeue jobs. Blocks when empty.</summary>
        IAsyncEnumerable<IngestionJob> DequeueAllAsync(CancellationToken cancellationToken);
    }
}
