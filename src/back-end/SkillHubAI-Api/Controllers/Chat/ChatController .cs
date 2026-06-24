using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SkillHubAI_Api.Models;
using SkillHubAI_Api.Services.Agent;
using SkillHubAI_Api.Services.Chat;
using System.Text;
using System.Text.Json;

namespace SkillHubAI_Api.Controllers.Chat
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        // private readonly IChatSessionService _sessionService;
        //private readonly IRagAgent _ragAgent;
        private readonly IAgentService _agentService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(
           // IChatSessionService sessionService,
           // IRagAgent ragAgent,
           IAgentService agentService,
            ILogger<ChatController> logger)
        {
            // _sessionService = sessionService;
            // _ragAgent = ragAgent;
            _agentService = agentService;
            _logger = logger;
        }

        [HttpPost("sessions")]
        public async Task<IActionResult> CreateSession(CancellationToken cancellationToken)
        {
            var (sessionId, info) = await _agentService.CreateSessionAsync(cancellationToken);

            return Ok(new
            {
                sessionId,
                title = info.Title,
                createdAt = info.CreatedAt
            });
        }

    
        [HttpGet("sessions/{sessionId}")]
        public async Task<IActionResult> GetSessions(string sessionId, CancellationToken cancellationToken)
        {
            var info = await _agentService.GetSessionInfoAsync(sessionId, cancellationToken);
            if (info is null)
                return NotFound(new { error = "Session not found" });

            return Ok(info);
        }

  
   /*     [HttpGet("sessions/{sessionId}/messages")]
        public async Task<IActionResult> GetMessages(string sessionId, CancellationToken cancellationToken)
        {
            var session = await _sessionService.GetSessionAsync(sessionId, cancellationToken);
            if (session is null)
                return NotFound(new { error = "Session not found" });

            var messages = await _sessionService.GetRecentMessagesAsync(
                sessionId, count: 50, cancellationToken);

            return Ok(messages.Select(m => new
            {
                id = m.Id,
                role = m.Role,
                content = m.Content,
                citations = m.Citations,
                timestamp = m.Timestamp
            }));
        }

       
        [HttpPost("messagestream")]
        public async Task StreamMessage([FromBody] ChatRequest request, CancellationToken cancellationToken)
        {
          
            if (string.IsNullOrWhiteSpace(request.SessionId) || string.IsNullOrWhiteSpace(request.Message))
            {
                Response.StatusCode = 400;
                await Response.WriteAsync(
                    JsonSerializer.Serialize(new { error = "SessionId and Message are required" }),
                    cancellationToken);
                return;
            }

            // check duplicate message
            var session = await _sessionService.GetSessionAsync(request.SessionId, cancellationToken);
            if (session is null)
            {
                Response.StatusCode = 404;
                await Response.WriteAsync(
                    JsonSerializer.Serialize(new { error = "Session not found" }),
                    cancellationToken);
                return;
            }

            _logger.LogInformation(
                "Chat message — Session: {SessionId}, Message: {Message}",
                request.SessionId, request.Message);

            // Save
            var userMessage = new ChatMessage
            {
                SessionId = request.SessionId,
                Role = "user",
                Content = request.Message,
                Timestamp = DateTime.UtcNow
            };
            await _sessionService.SaveMessageAsync(userMessage, cancellationToken);

            var history = await _sessionService.GetRecentMessagesAsync(
                request.SessionId, count: 10, cancellationToken);

            Response.ContentType = "text/event-stream";
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["Connection"] = "keep-alive";

     
            var fullResponse = new StringBuilder();

            await foreach (var token in _ragAgent.ChatStreamAsync(
                request.Message, history, cancellationToken))
            {
                fullResponse.Append(token);

                var sseData = JsonSerializer.Serialize(new { type = "token", content = token });
                await Response.WriteAsync($"data: {sseData}\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
            var citations = _ragAgent.GetLastCitations();
            var citationData = JsonSerializer.Serialize(new
            {
                type = "citations",
                citations = citations.Select(c => new
                {
                    sourceFileId = c.SourceFileId,
                    sourceFileName = c.SourceFileName,
                    chunkContent = c.ChunkContent,
                    relevanceScore = c.RelevanceScore
                })
            });
            await Response.WriteAsync($"data: {citationData}\n\n", cancellationToken);
            await Response.WriteAsync("data: [DONE]\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);

            var assistantMessage = new ChatMessage
            {
                SessionId = request.SessionId,
                Role = "assistant",
                Content = fullResponse.ToString(),
                Citations = citations,
                Timestamp = DateTime.UtcNow
            };
            await _sessionService.SaveMessageAsync(assistantMessage, cancellationToken);

            if (session.MessageCount == 0)
            {
                session.Title = request.Message.Length > 50
                    ? request.Message[..50] + "..."
                    : request.Message;
            }
            session.MessageCount += 2; // user + assistant
            await _sessionService.UpdateSessionAsync(session, cancellationToken);
        }
*/

        /// <summary>
        /// Send a message and get a complete response (non-streaming).
        /// </summary>
        [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.SessionId) || string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { error = "SessionId and Message are required" });

            var sessionInfo = await _agentService.GetSessionInfoAsync(request.SessionId, cancellationToken);
            if (sessionInfo is null)
                return NotFound(new { error = "Session not found" });

            _logger.LogInformation("Chat — Session: {SessionId}, Message: {Message}",
                request.SessionId, request.Message);

            var response = await _agentService.ChatAsync(
                request.SessionId, request.Message, cancellationToken);

            var citations = await _agentService.GetLastCitationsAsync();

            return Ok(new
            {
                sessionId = request.SessionId,
                response,
                citations = citations.Select(c => new
                {
                    sourceFileId = c.SourceFileId,
                    sourceFileName = c.SourceFileName,
                    chunkContent = c.ChunkContent,
                    relevanceScore = c.RelevanceScore
                })
            });
        }



    }
}
