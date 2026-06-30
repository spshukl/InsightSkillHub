namespace RagWebApp.Model
{
    public class SessionResponse
    {
        public string SessionId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
          public int MessageCount { get; set; }
    }
}
