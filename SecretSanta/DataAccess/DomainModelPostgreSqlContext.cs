using Microsoft.EntityFrameworkCore;
using SecretSanta.DataAccess.Models;

namespace SecretSanta.DataAccess {
    public class DomainModelPostgreSqlContext : DbContext {
        public DomainModelPostgreSqlContext(DbContextOptions<DomainModelPostgreSqlContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        public DbSet<Match> Matches { get; set; }

        public DbSet<MatchRestriction> MatchRestrictions { get; set; }

        public DbSet<Setting> Settings { get; set; }

        public DbSet<Session> Sessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<User>().HasKey(m => m.Id); //I think this makes it only save if the id was assigned?
            modelBuilder.Entity<Match>().HasKey(m => m.Id);
            modelBuilder.Entity<MatchRestriction>().HasKey(m => m.Id);

            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges() {
            ChangeTracker.DetectChanges();

            //the example uses this point to update an "UpdatedTimestamp" field

            return base.SaveChanges();
        }
    }
}
