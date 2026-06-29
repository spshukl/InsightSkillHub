using SkillHubAI_Api.Models;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace SkillHubAI_Api.Services.Queue
{
    public sealed class IngestionQueue : IIngestionQueue
    {
        private readonly Channel<IngestionRequest> _channel;

        public IngestionQueue()
        {
            var options = new BoundedChannelOptions(capacity: 100)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,  
                SingleWriter = false   
            };
            _channel = Channel.CreateBounded<IngestionRequest>(options);
        }

        public async ValueTask EnqueueAsync(
            IngestionRequest req,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(req);
            await _channel.Writer.WriteAsync(req, cancellationToken);
        }

        public async IAsyncEnumerable<IngestionRequest> DequeueAllAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var req in _channel.Reader.ReadAllAsync(cancellationToken))
            {
                yield return req;
            }
        }
    }
}
