namespace RagWebApp.Model
{
    public class ChatResponse
    {
        public string SessionId { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public List<Citation> Citations { get; set; } = [];
    }
}
