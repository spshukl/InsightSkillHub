namespace SkillHubAI_Api.Services.Storage
{
    /* public interface IAzureStorageService
     {
         Task UploadFileAsync(IFormFile file,string fileId);
     }*/

    public interface IAzureStorageService
    {
        /// <summary>
        /// Uploads file to Blob Storage, saves metadata to Cosmos DB,
        /// and enqueues the ingestion job for background processing.
        /// </summary>
        Task<IngestionMetadata> UploadFileAsync(
            IFormFile file,
            string fileId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Downloads blob content as a stream using authenticated client.
        /// Used by the background ingestion service.
        /// </summary>
        Task<Stream> DownloadBlobAsync(
            string blobUri,
            CancellationToken cancellationToken = default);
    }
}