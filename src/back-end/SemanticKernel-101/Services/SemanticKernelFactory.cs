using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using SemanticKernel_101.Configuration;
using SemanticKernel_101.Filters;
using SemanticKernel_101.Plugins;

namespace SemanticKernel_101.Services
{
    internal static class KernelFactory
    {
        public static Kernel CreateKernel(IOptions<AzureOpenAISettings> options, ILoggerFactory loggerFactory)
        {
            var settings = options.Value;

            var builder = Kernel.CreateBuilder();

            builder.Services.AddSingleton<ILoggerFactory>(loggerFactory);

            builder.AddAzureOpenAIChatCompletion(
                deploymentName: settings.Deployment,
                endpoint: settings.EndPoint,
                apiKey: settings.ApiKey);

            builder.Plugins.AddFromType<FoodPlugin>("foodplugins");  //type
            builder.Plugins.AddFromObject(new JokePlugin());// object
            builder.Plugins.AddFromObject(new SportPlugin());

            var kernel = builder.Build();

            kernel.FunctionInvocationFilters.Add(
                new LoggingFunctionFilter(loggerFactory.CreateLogger<LoggingFunctionFilter>()));

            return kernel;
        }
    }
}
