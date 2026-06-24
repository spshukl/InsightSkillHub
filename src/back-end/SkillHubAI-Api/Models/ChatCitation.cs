using Newtonsoft.Json;

namespace SkillHubAI_Api.Models
{
    public class ChatCitation
    {
        [JsonProperty("sourceFileId")]
        public string SourceFileId { get; set; } = string.Empty;

        [JsonProperty("sourceFileName")]
        public string SourceFileName { get; set; } = string.Empty;

        [JsonProperty("chunkContent")]
        public string ChunkContent { get; set; } = string.Empty;

        [JsonProperty("relevanceScore")]
        public double RelevanceScore { get; set; }
    }
}
