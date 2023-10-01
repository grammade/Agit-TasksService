using TaskService.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;

namespace TaskService.Models
{
    public class ChangelogEntry
    {
        public ChangelogEntry(EntityEntry entry)
        {
            Entry = entry;
        }

        public EntityEntry Entry { get; }
        public string Method { get; set; }
        public string TableName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string IP { get; set; }
        public Dictionary<string, object> KeyValues { get; } = new Dictionary<string, object>();
        public Dictionary<string, object> OldValues { get; } = new Dictionary<string, object>();
        public Dictionary<string, object> NewValues { get; } = new Dictionary<string, object>();
        public List<PropertyEntry> TemporaryProperties { get; } = new List<PropertyEntry>();
        public bool HasTemporaryProperties => TemporaryProperties.Any();
        public Changelog ToAudit()
        {
            var changelog = new Changelog();
            changelog.TableName = TableName;
            changelog.CreatedDate = CreatedDate;
            changelog.CreatedBy = CreatedBy;
            changelog.IPAddress = IP;
            changelog.Method = Method;
            changelog.KeyValues = JsonConvert.SerializeObject(KeyValues);
            changelog.OldValues = OldValues.Count == 0 ? null : JsonConvert.SerializeObject(OldValues);
            changelog.NewValues = NewValues.Count == 0 ? null : JsonConvert.SerializeObject(NewValues);
            return changelog;
        }
    }
}
