using SkillHubAI_Api.Models;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace SkillHubAI_Api.Services.Queue
{
    public sealed class IngestionQueue : IIngestionQueue
    {
        private readonly Channel<IngestionJob> _channel;

        public IngestionQueue()
        {
            var options = new BoundedChannelOptions(capacity: 100)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,  
                SingleWriter = false   
            };
            _channel = Channel.CreateBounded<IngestionJob>(options);
        }

        public async ValueTask EnqueueAsync(
            IngestionJob job,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(job);
            await _channel.Writer.WriteAsync(job, cancellationToken);
        }

        public async IAsyncEnumerable<IngestionJob> DequeueAllAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var job in _channel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return job;
            }
        }
    }
}
