using Microsoft.EntityFrameworkCore;
using TaskService.Entities;
using TaskService.Models;
using TaskService.Services;
using Task = System.Threading.Tasks.Task;

namespace TaskService.Persistence
{
    public class Context : DbContext
    {
        private readonly UserAuthorizationService _auth;
        public Context(DbContextOptions<Context> options,
            UserAuthorizationService auth)
            : base(options)
        {
            _auth = auth;
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Entities.Task> Tasks => Set<Entities.Task>();
        public DbSet<Changelog> Changelogs => Set<Changelog>();

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var changelogEntries = OnBeforeSaveChanges();   //prep changelog
            var res = await base.SaveChangesAsync(cancellationToken);
            await OnAfterSaveChanges(changelogEntries);     //get final temp values,save changes to changelog table
            return res;
        }


        private List<ChangelogEntry> OnBeforeSaveChanges()
        {
            ChangeTracker.DetectChanges();
            var _utcNow = DateTime.UtcNow;
            var changelogEntries = new List<ChangelogEntry>();
            var modifiedEntries = ChangeTracker.Entries()
                .Where(e => !(e.Entity is Changelog) && !e.State.Equals(EntityState.Detached) && !e.State.Equals(EntityState.Unchanged));

            foreach (var entry in modifiedEntries)
            {
                var changelogEntry = new ChangelogEntry(entry);
                changelogEntry.CreatedDate = _utcNow;
                changelogEntry.TableName = entry.Metadata.GetTableName();
                changelogEntry.Method = entry.State.ToString();
                changelogEntry.CreatedBy = _auth.getAuthorizedUser().UserName;
                changelogEntry.IP = _auth.getAuthorizedUser().IP;

                var modifiedProperty = entry.Properties;
                if (entry.State.Equals(EntityState.Modified))
                    modifiedProperty = modifiedProperty.Where(p => p.IsModified && p is not null).ToList();

                var pk = entry.Properties.Where(p => p.Metadata.IsPrimaryKey()).FirstOrDefault();
                changelogEntry.KeyValues[pk.Metadata.Name] = pk?.CurrentValue;

                foreach (var property in modifiedProperty)
                {
                    if (property.IsTemporary)
                    {
                        changelogEntry.TemporaryProperties.Add(property);
                        continue;
                    }

                    string propertyName = property.Metadata.Name;
                    switch (entry.State)
                    {
                        case EntityState.Added:
                            changelogEntry.NewValues[propertyName] = property.CurrentValue;
                            break;

                        case EntityState.Deleted:
                            changelogEntry.OldValues[propertyName] = property.OriginalValue;
                            break;

                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                changelogEntry.OldValues[propertyName] = property.OriginalValue;
                                changelogEntry.NewValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }
                changelogEntries.Add(changelogEntry);
            }

            // Save changelog entities that have all the modifications
            foreach (var ChangelogEntry in changelogEntries.Where(_ => !_.HasTemporaryProperties))
            {
                Changelogs.Add(ChangelogEntry.ToAudit());
            }

            // keep a list of entries where the value of some properties are unknown at this step
            return changelogEntries.Where(_ => _.HasTemporaryProperties).ToList();
        }

        private Task OnAfterSaveChanges(List<ChangelogEntry> changelogEntries)
        {
            if (changelogEntries == null || changelogEntries.Count == 0)
                return Task.CompletedTask;

            foreach (var ChangelogEntry in changelogEntries)
            {
                foreach (var prop in ChangelogEntry.TemporaryProperties)
                {
                    if (prop.Metadata.IsPrimaryKey())
                    {
                        ChangelogEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                    else
                    {
                        ChangelogEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                }

                // Save the Audit entry
                Changelogs.Add(ChangelogEntry.ToAudit());
            }

            return SaveChangesAsync();
        }
    }
}
