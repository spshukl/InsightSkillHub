using Azure;
using Azure.AI.OpenAI;

//using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using SkillHubAI_Api.Configurations.extensions;


var builder = WebApplication.CreateBuilder(args);

WebApplicationBuilderExtensions.ConfigureServices(builder);

var app = builder.Build();

WebApplicationExtensions.ConfigureCors(app);

app.Run();
