using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SemanticKernel_101.Configuration;
using SemanticKernel_101.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.SetBasePath(AppContext.BaseDirectory)
              
              .AddJsonFile("appsettings.Development.json", optional: true)
              .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<AzureOpenAISettings>(
            context.Configuration.GetSection(AzureOpenAISettings.SectionName));

        services.AddSingleton(sp =>
            KernelFactory.CreateKernel(
                sp.GetRequiredService<IOptions<AzureOpenAISettings>>(),
                sp.GetRequiredService<ILoggerFactory>()));

        services.AddSingleton<IChatService, ChatService>();
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Information);
    })
    .Build();

var chatService = host.Services.GetRequiredService<IChatService>();
await chatService.RunChatLoopAsync();
