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
    public DbSet<Event> Events { get; set; }
    public DbSet<EventAdmin> EventAdmins { get; set; }
    public DbSet<UserEvent> UserEvents { get; set; }
    public DbSet<EventType> EventTypes { get; set; }
    public DbSet<EventItem> EventItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
      modelBuilder.Entity<User>().HasKey(m => m.Id); //I think this makes it only save if the id was assigned?
      modelBuilder.Entity<Match>().HasKey(m => m.Id);
      modelBuilder.Entity<MatchRestriction>().HasKey(m => m.Id);
      modelBuilder.Entity<Event>().HasKey(m => m.Id);
      modelBuilder.Entity<EventAdmin>().HasKey(m => m.Id);
      modelBuilder.Entity<UserEvent>().HasKey(m => m.Id);
      modelBuilder.Entity<EventType>().HasKey(m => m.Id);
      modelBuilder.Entity<EventItem>().HasKey(m => m.Id);

      base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges() {
      ChangeTracker.DetectChanges();

      //the example uses this point to update an "UpdatedTimestamp" field

      return base.SaveChanges();
    }
  }
}
