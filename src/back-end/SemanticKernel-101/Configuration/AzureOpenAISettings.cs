using System;
using System.Collections.Generic;
using System.Text;

namespace SemanticKernel_101.Configuration
{
    public class AzureOpenAISettings
    {
        public const string SectionName = "AzureOpenAI";

        public string ApiKey { get; set; } = string.Empty;

        public string EndPoint { get; set; } = string.Empty;

        public string Deployment { get; set; } = string.Empty;
    }
}
