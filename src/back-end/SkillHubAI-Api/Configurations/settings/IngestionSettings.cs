using System.ComponentModel.DataAnnotations;

namespace SkillHubAI_Api.Configurations.settings
{
    public class IngestionSettings
    {
        public string ChunkingStrategy { get; set; } = "Header";
        public int MaxTokensPerChunk { get; set; } = 512;
        public int OverlapTokens { get; set; } = 50;
        [Range(1, 50)]
        public int EnricherBatchSize { get; set; } = 5;
    }
}
