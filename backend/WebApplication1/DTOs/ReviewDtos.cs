namespace OutsourcingApplication.DTOs
{
    public class ReviewSubmitDto
    {
        public int TaskId { get; set; }
        public string? GitUrl { get; set; }
        public IFormFile? ArchiveFile { get; set; }
        public IFormFile? DocFile { get; set; }
    }

    public class ReviewListDto
    {
        public int ReviewId { get; set; }
        public int TaskId { get; set; }
        public string TaskName { get; set; }
        public int Version { get; set; }
        public string? ResultName { get; set; } 
        public byte Result { get; set; }
        public string? Comment { get; set; }
        public string? PmName { get; set; } 
        public DateTime ReviewTime { get; set; }
        public string? GitUrl { get; set; }
        public string? ArchiveUrl { get; set; }
        public string? DocUrl { get; set; }
    }
}
