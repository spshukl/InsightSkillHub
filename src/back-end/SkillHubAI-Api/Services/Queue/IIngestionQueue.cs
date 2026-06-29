using SkillHubAI_Api.Models;

namespace SkillHubAI_Api.Services.Queue
{
    public interface IIngestionQueue
    {
        ValueTask EnqueueAsync(IngestionRequest job, CancellationToken cancellationToken = default);

        IAsyncEnumerable<IngestionRequest> DequeueAllAsync(CancellationToken cancellationToken);
    }
}
