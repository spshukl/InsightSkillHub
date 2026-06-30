namespace RagWebApp.Model
{
    public class ChatMessageViewModel
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<Citation> Citations { get; set; } = [];
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
