namespace TaskService.Entities
{
    public class Changelog
    {
        public Guid Id { get; set; }
        public string Method { get; set; }
        public string TableName { get; set; }
        public string? KeyValues { get; set; }
        public string? NewValues { get; set; }
        public string? OldValues { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? IPAddress { get; set; }
    }
}
