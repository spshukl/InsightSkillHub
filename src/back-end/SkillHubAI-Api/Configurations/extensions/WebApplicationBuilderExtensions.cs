using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenAI;
//using SkillHubAI_Api.Configurations.configs;
using SkillHubAI_Api.Configurations.settings;
using SkillHubAI_Api.Services.Agent;
using SkillHubAI_Api.Services.Chat;

//using SkillHubAI_Api.Controllers.DataSource;
using SkillHubAI_Api.Services.Ingestion;
using SkillHubAI_Api.Services.Queue;
using SkillHubAI_Api.Services.Status;
using SkillHubAI_Api.Services.Storage;
using System.ClientModel;

namespace SkillHubAI_Api.Configurations.extensions
{
    public static class WebApplicationBuilderExtensions
    {
        public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
        {
            var config = builder.Configuration;



            builder.Services
                .AddOptions<AzureBlobStorageSettings>()
                .Bind(config.GetSection("AzureBlobStorage"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services
                .AddOptions<CosmosDbSettings>()
                .Bind(config.GetSection("CosmosDb"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services
                .AddOptions<AzureOpenAISettings>()
                .Bind(config.GetSection("AzureOpenAI"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services
                .AddOptions<AzureAISearchSettings>()
                .Bind(config.GetSection("AzureAISearch"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services
                .AddOptions<IngestionSettings>()
                .Bind(config.GetSection("Ingestion"))
                .ValidateDataAnnotations()
                .ValidateOnStart();


            builder.Services.AddSingleton(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<AzureBlobStorageSettings>>().Value;
                return new BlobServiceClient(settings.ConnectionString);
            });

            // Azure Cosmos DB
            builder.Services.AddSingleton(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<CosmosDbSettings>>().Value;
                return new CosmosClient(settings.ConnectionString, new CosmosClientOptions
                {
                    SerializerOptions = new CosmosSerializationOptions
                    {
                        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                    },
                    ConnectionMode = ConnectionMode.Direct,
                    ApplicationName = "SkillHubAI"
                });
            });

            // Azure AI Search
            builder.Services.AddSingleton(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<AzureAISearchSettings>>().Value;
                return new SearchIndexClient(
                    new Uri(settings.Endpoint),
                    new AzureKeyCredential(settings.ApiKey));
            });
            builder.Services.AddSingleton(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<AzureAISearchSettings>>().Value;
                return new SearchClient(
                    new Uri(settings.Endpoint),
                    settings.IndexName,
                    new AzureKeyCredential(settings.ApiKey));
            });
            // Azure OpenAI
            builder.Services.AddSingleton(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<AzureOpenAISettings>>().Value;
                return new AzureOpenAIClient(
                    new Uri(settings.Endpoint),
                    new ApiKeyCredential(settings.ApiKey));
            });

            // Embedding Generator
            builder.Services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
            {
                var client = sp.GetRequiredService<AzureOpenAIClient>();
                var settings = sp.GetRequiredService<IOptions<AzureOpenAISettings>>().Value;
                return client
                    .GetEmbeddingClient(settings.EmbeddingDeployment)
                    .AsIEmbeddingGenerator();
            });
            builder.Services.AddSingleton<IChatClient>(sp =>
            {
                var client = sp.GetRequiredService<AzureOpenAIClient>();
                var settings = sp.GetRequiredService<IOptions<AzureOpenAISettings>>().Value;

                return new ChatClientBuilder(
                        client.GetChatClient(settings.ChatDeployment).AsIChatClient())
                    .ConfigureOptions(options =>
                    {
                        options.MaxOutputTokens ??= 16384;
                    })
                    .Build();
            });

            builder.Services.AddSingleton<IIngestionQueue, IngestionQueue>();


            builder.Services.AddScoped<IAzureStorageService, AzureStorageServiceImp>();
            builder.Services.AddScoped<IIngestionStatusHandler, IngestionStatusImp>();
            // ─── Agent Framework Services ───
            builder.Services.AddSingleton<CosmosDbChatHistoryProvider>();
            builder.Services.AddSingleton<IAgentService, AgentService>();
            builder.Services.AddSingleton<IKnowledgeRetriever, KnowledgeRetriever>();

            //builder.Services.AddScoped<IChatSessionService, ChatSessionService>();
            // builder.Services.AddScoped<IRagAgent, RagAgent >();
            //builder.Services.AddScoped<IKnowledgeRetriever, KnowledgeRetriever>();

            builder.Services.AddHostedService<DataIngestionService>();

            //     ═════════════════════════════════════════════════════

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHealthChecks();

            return builder;
        }
    }
}