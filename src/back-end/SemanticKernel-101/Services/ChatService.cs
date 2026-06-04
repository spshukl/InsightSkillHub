using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace SemanticKernel_101.Services
{
    internal sealed class ChatService : IChatService
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatCompletionService;
        private readonly ILogger<ChatService> _logger;
        private readonly ChatHistory _history;

        private const string SystemPrompt = """
            You are a helpful assistant with access to plugins for jokes, food, and sports.
            Before calling a plugin, always ask the user which specific type they want:
            - For jokes: ask if they want Programming, Dad Joke, or One-Liner.
            - For food: ask if they want Indian, Continental, or Italian.
            - For sports: ask if they want Indoor or Outdoor.
            Only call the plugin after the user confirms their choice.
            """;

        public ChatService(Kernel kernel, ILogger<ChatService> logger)
        {
            _kernel = kernel;
            _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();
            _logger = logger;
            _history = new ChatHistory(SystemPrompt);
        }

        public async Task RunChatLoopAsync(CancellationToken cancellationToken = default)
        {
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            Console.WriteLine("Chat started. Type 'exit' to quit.");
            Console.WriteLine();

            while (!cancellationToken.IsCancellationRequested)
            {
                Console.Write("You: ");
                var userInput = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(userInput))
                {
                    continue;
                }

                if (userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                _logger.LogInformation("User prompt: {Prompt}", userInput);
                _history.AddUserMessage(userInput);

                Console.Write("Assistant: ");

                var stopwatch = Stopwatch.StartNew();
                var fullResponse = new System.Text.StringBuilder();

                /*      await foreach (var chunk in _chatCompletionService.GetStreamingChatMessageContentsAsync(
                          _history, executionSettings, _kernel, cancellationToken))
                      {
                          if (!string.IsNullOrEmpty(chunk.Content))
                          {
                              Console.Write(chunk.Content);
                              fullResponse.Append(chunk.Content);
                          }
                      }*/
/*                _kernel.Plugins.AddFromFunctions("health",
[
    KernelFunctionFactory.CreateFromMethod(
        method: () => "go to sleep",
        functionName: "health_pluggins",
        description: "get the health information"
    )]);*/

               var response = await _chatCompletionService.GetChatMessageContentsAsync(_history, executionSettings, _kernel, cancellationToken);

               // Assuming response is a collection of ChatMessageContent, print the content of the first message
               if (response is not null && response.Count > 0 && !string.IsNullOrEmpty(response[0].Content))
               {
                   Console.WriteLine($"Streaming response: {response[0].Content}");
               }
               else
               {
                   Console.WriteLine("Streaming response: [No content returned]");
               }

                stopwatch.Stop();
                Console.WriteLine();
                Console.WriteLine();

                var assistantText = fullResponse.ToString();
                _history.AddAssistantMessage(assistantText);

                _logger.LogInformation("Assistant response ({Duration}ms): {Response}",
                    stopwatch.ElapsedMilliseconds, assistantText);
            }
        }
    }
}
