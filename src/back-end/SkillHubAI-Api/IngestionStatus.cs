namespace SkillHubAI_Api
{
    public enum IngestionStatus
    {
     
        Uploaded,
        Queued,
        Extracting,
        Chunking,
        Embedding,
        Storing,
        Completed,
        Failed
    }
}
