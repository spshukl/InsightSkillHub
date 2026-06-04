using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace SemanticKernel_101.Filters
{
    internal sealed class LoggingFunctionFilter : IFunctionInvocationFilter
    {
        private readonly ILogger _logger;

        public LoggingFunctionFilter(ILogger logger)
        {
            _logger = logger;
        }

        public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
        {
            _logger.LogInformation("Function invoking: {PluginName}.{FunctionName}",
                context.Function.PluginName, context.Function.Name);

            var stopwatch = Stopwatch.StartNew();

            await next(context);

            stopwatch.Stop();

            _logger.LogInformation("Function completed: {PluginName}.{FunctionName} in {Duration}ms | Result: {Result}",
                context.Function.PluginName,
                context.Function.Name,
                stopwatch.ElapsedMilliseconds,
                context.Result?.ToString() ?? "(null)");
        }
    }
}
