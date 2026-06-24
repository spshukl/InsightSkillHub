using System.ComponentModel.DataAnnotations;

namespace SkillHubAI_Api.Configurations.settings
{
    public class CosmosDbSettings
    {
        [Required(ErrorMessage = "CosmosDb:ConnectionString is required. Add it to appsettings.Development.json")]
        public string ConnectionString { get; set; } = string.Empty;

        [Required]
        public string DatabaseName { get; set; } = "SkillHubAI";

        [Required]
        public string ContainerName { get; set; } = "IngestionStatus";
    }
}
