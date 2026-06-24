namespace SkillHubAI_Api.Services.Agent
{
    public class AgentSessionInfo
    {
        public string SessionId { get; set; } = string.Empty;
        public string Title { get; set; } = "New Chat";
        public DateTime CreatedAt { get; set; }
        public int MessageCount { get; set; }
    }
}
