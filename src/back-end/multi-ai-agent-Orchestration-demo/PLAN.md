# Multi-AI Agent Orchestration - Product Recommendation

## Overview

AI-driven product recommendation system using **Semantic Kernel Agents** in C# / .NET 10.

## Architecture

```
User ? Orchestrator ? Preference Collector ? Product Search ? Review Analyst ? Purchase Advisor ? User
```

## Agents

| Agent | Role | Input | Output |
|-------|------|-------|--------|
| **Orchestrator** | Routes between agents | User message | Delegation |
| **Preference Collector** | Gathers: budget, color, brand, size, type, occasion | Chat | `UserPreferences` |
| **Product Search** | Searches internal catalog | `UserPreferences` | `List<Product>` |
| **Review Analyst** | Scores & summarizes reviews | `List<Product>` | `List<RankedProduct>` |
| **Purchase Advisor** | Presents top picks, confirms purchase | `List<RankedProduct>` | Decision |

## Data Models

```csharp
UserPreferences { Budget, Color, Brand, Size, Type, Occasion }
Product { Name, Price, Brand, Platform, Rating, Url }
RankedProduct : Product { ReviewSummary, Score, Pros, Cons }
```

## Execution Flow

1. User: *"I want to buy shoes"*
2. Orchestrator ? Preference Collector (asks budget, color, brand, size, occasion)
3. Preference Collector ? Product Search (structured preferences)
4. Product Search ? Review Analyst (candidate products)
5. Review Analyst ? Purchase Advisor (ranked results)
6. Purchase Advisor ? User (top 3 recommendations)
7. User confirms or refines ? loop back to step 2

## Project Structure

```
multi-ai-agent-Orchestration-demo/
??? Configuration/
?   ??? AzureOpenAISettings.cs
??? Models/
?   ??? UserPreferences.cs
?   ??? Product.cs
?   ??? RankedProduct.cs
??? Agents/
?   ??? OrchestratorAgent.cs
?   ??? PreferenceCollectorAgent.cs
?   ??? ProductSearchAgent.cs
?   ??? ReviewAnalystAgent.cs
?   ??? PurchaseAdvisorAgent.cs
??? Plugins/
?   ??? ProductSearchPlugin.cs
?   ??? ReviewPlugin.cs
??? Services/
?   ??? AgentOrchestrationService.cs
??? Data/
?   ??? MockProductCatalog.json
??? Program.cs
??? appsettings.Development.json
??? PLAN.md
```

## Tech Stack

- .NET 10 / C# 14
- Microsoft.SemanticKernel
- Microsoft.SemanticKernel.Agents.Core
- Microsoft.SemanticKernel.Connectors.AzureOpenAI
- Azure OpenAI (gpt-4o)

## NuGet Packages

```xml
<PackageReference Include="Microsoft.SemanticKernel" Version="1.*" />
<PackageReference Include="Microsoft.SemanticKernel.Agents.Core" Version="1.*" />
<PackageReference Include="Microsoft.SemanticKernel.Connectors.AzureOpenAI" Version="1.*" />
<PackageReference Include="Microsoft.Extensions.Hosting" Version="9.*" />
```

## Implementation Phases

### Phase 1 - Foundation
- [ ] Add NuGet packages
- [ ] Create config (`AzureOpenAISettings`)
- [ ] Define models (`UserPreferences`, `Product`, `RankedProduct`)
- [ ] Create mock product catalog JSON

### Phase 2 - Agents
- [ ] `PreferenceCollectorAgent` — conversational preference gathering
- [ ] `ProductSearchPlugin` — search mock catalog by preferences
- [ ] `ReviewAnalystAgent` — LLM-based review synthesis
- [ ] `PurchaseAdvisorAgent` — present recommendations

### Phase 3 - Orchestration
- [ ] `AgentGroupChat` with selection strategy
- [ ] Termination strategy (purchase confirmed / exit)
- [ ] Wire up in `Program.cs`

### Phase 4 - Enhancements
- [ ] External search APIs
- [ ] Conversation persistence
- [ ] Streaming responses

## Key SK Concepts

- `ChatCompletionAgent` — agent with system prompt + kernel
- `AgentGroupChat` — multi-agent turn-based chat
- `KernelFunction` — plugins for search/reviews
- `KernelFunctionSelectionStrategy` — which agent speaks next
- `KernelFunctionTerminationStrategy` — when to stop
