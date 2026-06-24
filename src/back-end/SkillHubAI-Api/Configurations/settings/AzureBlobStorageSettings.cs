using System.ComponentModel.DataAnnotations;

namespace SkillHubAI_Api.Configurations.settings
{
    public class AzureBlobStorageSettings
    {
        /// <summary>
        /// Full connection string including AccountKey.
        /// Loaded from appsettings.Development.json (gitignored).
        /// </summary>
        [Required(ErrorMessage = "AzureBlobStorage:ConnectionString is required. Add it to appsettings.Development.json")]
        public string ConnectionString { get; set; } = string.Empty;

        [Required]
        public string ContainerName { get; set; } = "datasource";
    }
}
