using System.ComponentModel.DataAnnotations;

namespace SkillHubAI_Api.Configurations.settings
{
    public class AzureAISearchSettings
    {
        [Required(ErrorMessage = "AzureAISearch:Endpoint is required. Add it to appsettings.Development.json")]
        [Url]
        public string Endpoint { get; set; } = string.Empty;

        [Required(ErrorMessage = "AzureAISearch:ApiKey is required. Add it to appsettings.Development.json")]
        public string ApiKey { get; set; } = string.Empty;

        [Required]
        public string IndexName { get; set; } = "skillhubai-index";
    }
}
