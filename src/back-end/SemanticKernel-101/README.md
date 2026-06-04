# SemanticKernel-101

A .NET 10 console application demonstrating Microsoft Semantic Kernel with Azure OpenAI, featuring plugin-based chat with automatic function calling.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- An Azure OpenAI resource with a deployed chat model (e.g., `gpt-4o`)

## Configuration

1. Create or update `appsettings.Development.json` in the project root:

| Setting      | Description                                                                 |
|--------------|-----------------------------------------------------------------------------|
| `ApiKey`     | Your Azure OpenAI resource API key (found in Azure Portal → Keys & Endpoint) |
| `EndPoint`   | Your Azure OpenAI endpoint URL (do **not** append `/openai/v1`)             |
| `Deployment` | The deployment name you created in Azure AI Studio (e.g., `gpt-4o`)         |

> ⚠️ Do not commit your API key to source control. Consider using User Secrets or environment variables for production use.

## Running the Application

## Usage

Once running, you'll see an interactive chat prompt:
Chat started. Type 'exit' to quit.
You: tell me a joke Assistant: What kind of joke would you like? Programming, Dad Joke, or One-Liner? You: programming Assistant: Why do programmers prefer dark mode? Because light attracts bugs!

### Available Plugins

| Plugin | Description | Options |
|--------|-------------|---------|
| **Jokes** | Returns a joke by category | Programming, Dad Joke, One-Liner |
| **Food** | Suggests food by cuisine | Indian, Continental, Italian |
| **Sports** | Suggests sports by type | Indoor, Outdoor |

The assistant will ask you to pick a specific category before invoking a plugin.

Type `exit` to quit the application.

## Troubleshooting

| Error | Cause | Fix |
|-------|-------|-----|
| `HTTP 404 - Resource not found` | Incorrect endpoint or deployment name | Verify `EndPoint` has no trailing path and `Deployment` matches your Azure portal deployment name |
| `HTTP 401 - Unauthorized` | Invalid API key | Check your `ApiKey` value in Azure Portal → Keys & Endpoint |
| `HTTP 429 - Too Many Requests` | Rate limit exceeded | Wait and retry, or increase your Azure OpenAI quota |




