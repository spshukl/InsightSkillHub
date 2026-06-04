namespace SemanticKernel_101.Services
{
    internal interface IChatService
    {
        Task RunChatLoopAsync(CancellationToken cancellationToken = default);
    }
}
