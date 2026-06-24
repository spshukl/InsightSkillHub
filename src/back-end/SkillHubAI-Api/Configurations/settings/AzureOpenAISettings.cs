using System.ComponentModel.DataAnnotations;

namespace SkillHubAI_Api.Configurations.settings
{
    public class AzureOpenAISettings
    {
        [Required(ErrorMessage = "AzureOpenAI:Endpoint is required. Add it to appsettings.Development.json")]
        [Url]
        public string Endpoint { get; set; } = string.Empty;

        [Required(ErrorMessage = "AzureOpenAI:ApiKey is required. Add it to appsettings.Development.json")]
        public string ApiKey { get; set; } = string.Empty;

        [Required]
        public string EmbeddingDeployment { get; set; } = "text-embedding-3-small";

        [Range(1, 4096)]
        public int EmbeddingDimensions { get; set; } = 1536;

        [Required]
        public string ChatDeployment { get; set; } = "gpt-5.4";
        [Range(1000, 128000)]
        public int MaxOutputTokens { get; set; } = 16384;
    }
}
