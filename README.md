```markdown
# InsightSkillHub — Enterprise RAG Chat

A full-stack Retrieval-Augmented Generation (RAG) chat application built with .NET, Azure OpenAI, and Azure AI Search. Upload documents, ingest them into a vector store, and ask natural-language questions grounded in your own data.

## Architecture

```
?????????????????        HTTP        ????????????????????????
?  RagWebApp    ?  ????????????????  ?  SkillHubAI-Api      ?
?  (Blazor SSR) ?   localhost:5000   ?  (ASP.NET Core API)  ?
?????????????????                    ????????????????????????
                                                ?
                     ???????????????????????????????????????????????????????
                     ?                          ?                          ?
              ???????????????          ???????????????????       ???????????????????
              ? Azure Blob  ?          ? Azure OpenAI    ?       ? Azure AI Search ?
              ? Storage     ?          ? (GPT-4o)        ?       ? (Vector Index)  ?
              ???????????????          ???????????????????       ???????????????????
```

| Layer | Technology | Purpose |
|-------|-----------|---------|
| **Frontend** | .NET 10 Blazor Server | Interactive chat UI with session history, document upload, and citation display |
| **Backend API** | .NET 8 ASP.NET Core Web API | Chat orchestration, document ingestion, agent-based RAG pipeline |
| **Vector Store** | Azure AI Search | Stores chunked document embeddings for semantic retrieval |
| **LLM** | Azure OpenAI (GPT-4o) | Generates grounded responses using retrieved context |
| **Storage** | Azure Blob Storage | Stores uploaded documents for processing |
| **Chat History** | Azure Cosmos DB | Persists sessions and messages |

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (frontend)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (backend API)
- [Docker & Docker Compose](https://docs.docker.com/get-docker/)
- An **Azure OpenAI** resource with a deployed chat model (e.g., `gpt-4o`)
- An **Azure AI Search** resource
- An **Azure Blob Storage** account
- An **Azure Cosmos DB** account (optional — for chat history persistence)

## Project Structure

```
src/
??? back-end/
?   ??? SkillHubAI-Api/          # ASP.NET Core Web API (RAG backend)
?   ?   ??? Controllers/         # Chat & DataSource endpoints
?   ?   ??? Services/            # Agent, Chat, Ingestion, Storage services
?   ?   ??? Configurations/      # Settings & DI extensions
?   ?   ??? Models/              # Domain models
?   ??? SemanticKernel-101/      # Standalone Semantic Kernel demo
?   ??? enterprise-rag-chat-101/ # Placeholder console app
??? front-end/
?   ??? RagWebApp/               # Blazor Server chat UI
??? docker-compose.yml           # Orchestrates API + Web App
```

## Configuration

### Backend API (`src/back-end/SkillHubAI-Api/appsettings.Development.json`)

```json
{
  "AzureOpenAI": {
    "Endpoint": "<your-azure-openai-endpoint>",
    "ApiKey": "<your-api-key>",
    "DeploymentName": "gpt-4o"
  },
  "AzureAISearch": {
    "Endpoint": "<your-search-endpoint>",
    "ApiKey": "<your-search-api-key>",
    "IndexName": "skillhubai-index"
  },
  "AzureBlobStorage": {
    "ConnectionString": "<your-blob-connection-string>",
    "ContainerName": "documents"
  },
  "CosmosDb": {
    "ConnectionString": "<your-cosmos-connection-string>",
    "DatabaseName": "SkillHubAI",
    "ContainerName": "ChatSessions"
  }
}
```

### Frontend (`src/front-end/RagWebApp`)

The frontend reads the API base URL from the environment variable `ApiBaseUrl` (defaults to `http://agenticai-api:8080/` in Docker).

> ?? **Never commit API keys to source control.** Use User Secrets, environment variables, or Azure Key Vault for sensitive values.

## Running with Docker Compose

The easiest way to run the full stack:

```bash
cd src
docker-compose up --build
```

| Service | URL |
|---------|-----|
| Backend API | http://localhost:7001 |
| Frontend (RagWebApp) | http://localhost:5000 |

## Running Locally (without Docker)

### Backend API

```bash
cd src/back-end/SkillHubAI-Api
dotnet run
```

### Frontend

```bash
cd src/front-end/RagWebApp
dotnet run
```

Set the `ApiBaseUrl` environment variable to point to the running backend (e.g., `https://localhost:7001/`).

## API Endpoints

### Chat

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/chat/sessions` | Create a new chat session |
| `GET` | `/api/chat/sessions` | List all sessions |
| `GET` | `/api/chat/sessions/{sessionId}` | Get session details |
| `GET` | `/api/chat/sessions/{sessionId}/messages` | Get messages for a session |
| `POST` | `/api/chat/message` | Send a message and get a RAG-grounded response |

### Data Source / Ingestion

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/datasource` | Health check |
| `POST` | `/api/datasource/upload` | Upload a document for ingestion |
| `GET` | `/api/datasource/{uploadId}` | Poll ingestion status |

## Features

- **Conversational RAG** — Ask questions grounded in your uploaded documents with cited sources.
- **Document Ingestion** — Upload PDF, DOCX, TXT, or Markdown files; they are chunked, embedded, and indexed automatically.
- **Citation Display** — Responses include source references with relevance scores.
- **Session Management** — Multiple chat sessions with persistent history.
- **Agentic Architecture** — Uses Microsoft Agents AI for orchestration and function calling.

## Troubleshooting

| Error | Cause | Fix |
|-------|-------|-----|
| `HTTP 404` on chat endpoints | API not reachable or wrong base URL | Verify `ApiBaseUrl` points to the running backend |
| `HTTP 401 — Unauthorized` | Invalid Azure OpenAI / Search API key | Check keys in `appsettings.Development.json` |
| `HTTP 429 — Too Many Requests` | Azure OpenAI rate limit | Wait and retry, or increase quota |
| Ingestion stuck at "Queued" | Blob trigger or ingestion worker not running | Check blob storage connection and ingestion service logs |
| Docker build fails | Missing .NET SDK or Docker version mismatch | Ensure Docker is up to date and .NET SDKs are installed |

## License

This project is for educational and demonstration purposes.
```