using RagWebApp.Model;

namespace RagWebApp.Services
{
    public class RagApiService:IRagApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RagApiService> _logger;
        public RagApiService(HttpClient httpClient, ILogger<RagApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<SessionResponse?> CreateSessionAsync(CancellationToken ct = default)
        {
            var response = await _httpClient.PostAsync("api/Chat/sessions", null, ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<SessionResponse>(ct);
        }

        public async Task<List<SessionResponse>> GetAllSessionsAsync(CancellationToken ct = default)
        {
            return await _httpClient.GetFromJsonAsync<List<SessionResponse>>("api/Chat/sessions", ct) ?? [];
        }

        public async Task<IngestionStatus?> GetIngestionStatusAsync(string uploadId, CancellationToken ct = default)
        {
            return await _httpClient.GetFromJsonAsync<IngestionStatus>($"api/DataSource/{uploadId}", ct);
        }

        public async Task<List<ChatMessageViewModel>> GetSessionMessagesAsync(string sessionId, CancellationToken ct = default)
        {
            return await _httpClient.GetFromJsonAsync<List<ChatMessageViewModel>>(
            $"api/Chat/sessions/{sessionId}/messages", ct) ?? [];
        
        }

        public async Task<ChatResponse?> SendMessageAsync(string sessionId, string message, CancellationToken ct = default)
        {
            var request = new ChatRequest { SessionId = sessionId, Message = message };
            var response = await _httpClient.PostAsJsonAsync("api/Chat/message", request, ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ChatResponse>(ct);
        }

        public async Task<UploadResponse?> UploadDocumentAsync(Stream fileStream, string fileName, CancellationToken ct = default)
        {
            using var content = new MultipartFormDataContent();
            using var streamContent = new StreamContent(fileStream);
            content.Add(streamContent, "file", fileName);

            var response = await _httpClient.PostAsync("api/DataSource/upload", content, ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UploadResponse>(ct);
        }
    }
}
