using RagWebApp.Model;

namespace RagWebApp.Services
{
    public interface IRagApiService
    {
        Task<SessionResponse?> CreateSessionAsync(CancellationToken ct = default);
        Task<ChatResponse?> SendMessageAsync(string sessionId, string message, CancellationToken ct = default);
        Task<UploadResponse?> UploadDocumentAsync(Stream fileStream, string fileName, CancellationToken ct = default);
        Task<IngestionStatus?> GetIngestionStatusAsync(string uploadId, CancellationToken ct = default);
        Task<List<SessionResponse>> GetAllSessionsAsync(CancellationToken ct = default);
        Task<List<ChatMessageViewModel>> GetSessionMessagesAsync(string sessionId, CancellationToken ct = default);
    }
}
