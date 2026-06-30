namespace RagWebApp.Model
{
    public class Citation
    {
        public string SourceFileId { get; set; } = string.Empty;
        public string SourceFileName { get; set; } = string.Empty;
        public string ChunkContent { get; set; } = string.Empty;
        public double RelevanceScore { get; set; }
    }
}
