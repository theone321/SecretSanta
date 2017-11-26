using Microsoft.EntityFrameworkCore;
using SecretSanta.DataAccess.Models;

namespace SecretSanta.DataAccess {
    public class DomainModelPostgreSqlContext : DbContext {
        public DomainModelPostgreSqlContext(DbContextOptions<DomainModelPostgreSqlContext> options) : base(options) { }

        public DbSet<Name> Names { get; set; }

        public DbSet<Match> Matches { get; set; }

        public DbSet<MatchRestriction> MatchRestrictions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Name>().HasKey(m => m.Id); //I think this makes it only save if the id was assigned?

            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges() {
            ChangeTracker.DetectChanges();

            //the example uses this point to update an "UpdatedTimestamp" field

            return base.SaveChanges();
        }
    }
}
