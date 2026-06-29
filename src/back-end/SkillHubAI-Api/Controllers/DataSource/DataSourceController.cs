using Microsoft.AspNetCore.Mvc;
using SkillHubAI_Api.Services.Status;
using SkillHubAI_Api.Services.Storage;

namespace SkillHubAI_Api.Controllers.DataSource
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataSourceController : ControllerBase
    {
        private readonly IAzureStorageService _azStorageService;
        private readonly IIngestionStatusHandler _statusHandler;
        private readonly ILogger<DataSourceController> _logger;

        public DataSourceController(
            IAzureStorageService azStorageService,
            IIngestionStatusHandler statusHandler,
            ILogger<DataSourceController> logger)
        {
            _azStorageService = azStorageService;
            _statusHandler = statusHandler;
            _logger = logger;
        }

        /// <summary>
        /// Health check endpoint.
        /// </summary>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                service = "SkillHubAI DataSource",
                status = "Healthy",
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Poll the ingestion status of an uploaded document.
        /// </summary>
        [HttpGet("{uploadId}")]
        public async Task<IActionResult> GetUploadStatus(string uploadId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(uploadId))
                return BadRequest(new { error = "Upload ID is required." });

            var status = await _statusHandler.GetIngestionStatusAsync(uploadId, cancellationToken);

            if (status is null)
                return NotFound(new { error = $"Upload '{uploadId}' not found" });

            return Ok(new
            {
                uploadId = status.FileId,
                fileName = status.FileName,
                status = status.Status.ToString(),
                statusMessage = status.StatusMessage,
                chunkCount = status.ChunkCount,
                createdAt = status.CreatedAt,
                updatedAt = status.UpdatedAt,
                completedAt = status.CompletedAt
            });
        }

        /// <summary>
        /// Upload a document for ingestion into the RAG pipeline.
        /// Returns 202 Accepted with a status polling URL.
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file, CancellationToken cancellationToken)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { error = "No file uploaded" });

                string uploadId = Guid.NewGuid().ToString();

                _logger.LogInformation(
                    "starting file upload — UploadId: {UploadId}, File: {FileName}, Size: {Size} bytes",
                    uploadId, file.FileName, file.Length);

                await _azStorageService.UploadFileAsync(file, uploadId, cancellationToken);

                return AcceptedAtAction(
                    actionName: nameof(GetUploadStatus),
                    routeValues: new { uploadId },
                    value: new
                    {
                        uploadId,
                        fileName = file.FileName,
                        fileSize = file.Length,
                        status = "Queued",
                        message = "File uploaded and queued for ingestion",
                        statusUrl = Url.Action(nameof(GetUploadStatus), new { uploadId })
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
