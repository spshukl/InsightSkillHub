namespace SkillHubAI_Api.Services.Storage
{
    /* public interface IAzureStorageService
     {
         Task UploadFileAsync(IFormFile file,string fileId);
     }*/

    public interface IAzureStorageService
    {
       
        Task<IngestionMetadata> UploadFileAsync(
            IFormFile file,
            string fileId,
            CancellationToken cancellationToken = default);

     
        Task<Stream> DownloadBlobAsync(
            string blobUri,
            CancellationToken cancellationToken = default);
    }
}