using Newtonsoft.Json;

namespace SkillHubAI_Api.Models
{
        public class ChatSession
        {
            [JsonProperty("id")]
            public string Id { get; set; } = Guid.NewGuid().ToString();

            [JsonProperty("sessionId")]
            public string SessionId { get; set; } = string.Empty;

            [JsonProperty("type")]
            public string Type { get; set; } = "session";

            [JsonProperty("title")]
            public string Title { get; set; } = "New Chat";

            [JsonProperty("createdAt")]
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

            [JsonProperty("lastMessageAt")]
            public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

            [JsonProperty("messageCount")]
            public int MessageCount { get; set; } = 0;

            [JsonProperty("serializedSession")]
            public string? SerializedSession { get; set; }
        }
    }

