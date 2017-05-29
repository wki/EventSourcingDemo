using System.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Wki.EventSourcing.Persistence.Ef
{
    public class EventStoreContext: DbContext
    {
        public DbSet<EventRow> EventRows { get; set; }
        public DbSet<SnapshotRow> SnapshotRows { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["eventstore"].ConnectionString;
            optionsBuilder
                .UseNpgsql(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .ForNpgsqlUseSerialColumns();

            modelBuilder
                .Entity<EventRow>()
                .ToTable("event")
                ;

            modelBuilder
                .Entity<SnapshotRow>()
                .ToTable("snapshot");
        }
    }
}
