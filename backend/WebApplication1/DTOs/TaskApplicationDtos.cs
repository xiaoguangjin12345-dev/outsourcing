namespace OutsourcingApplication.DTOs
{
    public record TaskApplicationListDto
    {
        public int ApplicationID { get; set; }
        public int TaskId { get; set; } 
        public string TaskName { get; set; } = null!;
        public int DevID { get; set; }
        public string DevName { get; set; } = null!;
        public string? DevSkills { get; set; }
        public byte Type { get; set; } 
        public byte Status { get; set; }
        public DateTime ApplyTime { get; set; }
    }
}
