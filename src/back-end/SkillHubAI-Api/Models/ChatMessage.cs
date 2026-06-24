using Newtonsoft.Json;

namespace SkillHubAI_Api.Models
{
    public class ChatMessage
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("sessionId")]
        public string SessionId { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = "message";

        /// <summary>
        /// "user" or "assistant"
        /// </summary>
        [JsonProperty("role")]
        public string Role { get; set; } = string.Empty;

        [JsonProperty("content")]
        public string Content { get; set; } = string.Empty;

        [JsonProperty("citations")]
        public List<ChatCitation>? Citations { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
