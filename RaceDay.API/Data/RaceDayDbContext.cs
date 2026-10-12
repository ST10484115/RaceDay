
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<EventLocation> EventLocations => Set<EventLocation>();
        public DbSet<Enrolment> Enrolments => Set<Enrolment>();
        public DbSet<Result> Results => Set<Result>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // USERS
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(u => u.UserId);
                entity.HasIndex(u => u.Email).IsUnique();

                entity.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.LastName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.Role).IsRequired().HasMaxLength(20);
                entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_Users_Role",
                    "[Role] IN ('Organiser', 'Participant')"));
            });

            // EVENT LOCATIONS
            modelBuilder.Entity<EventLocation>(entity =>
            {
                entity.ToTable("EventLocations");
                entity.HasKey(l => l.LocationId);

                entity.Property(l => l.LocationName).IsRequired().HasMaxLength(150);
                entity.Property(l => l.Address).IsRequired().HasMaxLength(250);
                entity.Property(l => l.City).IsRequired().HasMaxLength(100);
                entity.Property(l => l.Province).IsRequired().HasMaxLength(100);
                entity.Property(l => l.Latitude).HasColumnType("decimal(9,6)");
                entity.Property(l => l.Longitude).HasColumnType("decimal(9,6)");
            });

            // EVENTS
            modelBuilder.Entity<Event>(entity =>
            {
                entity.ToTable(t =>
                {
                    t.HasCheckConstraint(
                        "CK_Events_DistanceKm",
                        "[DistanceKm] > 0");

                    t.HasCheckConstraint(
                        "CK_Events_EventType",
                        "[EventType] IN ('Run', 'Walk', 'Cycle')");
                });

                entity.HasKey(e => e.EventId);
                entity.Property(e => e.EventName).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(30);
                entity.Property(e => e.DistanceKm).HasColumnType("decimal(6,2)");
                entity.Property(e => e.EventType).IsRequired().HasMaxLength(20);

                entity.HasOne(e => e.Organiser)
                    .WithMany(u => u.OrganisedEvents)
                    .HasForeignKey(e => e.OrganiserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Location)
                    .WithMany(l => l.Events)
                    .HasForeignKey(e => e.LocationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // CATEGORIES
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable(t =>
                    t.HasCheckConstraint("CK_Categories_DistanceKm", "[DistanceKm] > 0"));

                entity.HasKey(c => c.CategoryId);
                entity.Property(c => c.CategoryName).IsRequired().HasMaxLength(100);
                entity.Property(c => c.DistanceKm).HasColumnType("decimal(6,2)");
                entity.Property(c => c.EntryFee).HasColumnType("decimal(10,2)");

                entity.HasOne(c => c.Event)
                    .WithMany(e => e.Categories)
                    .HasForeignKey(c => c.EventId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ENROLMENTS
            modelBuilder.Entity<Enrolment>(entity =>
            {
                entity.ToTable("Enrolments");
                entity.HasKey(e => e.EnrolmentId);

                entity.Property(e => e.Status).IsRequired().HasMaxLength(30);

                entity.HasIndex(e => new { e.ParticipantId, e.CategoryId })
                    .IsUnique();

                entity.HasOne(e => e.Participant)
                    .WithMany(u => u.Enrolments)
                    .HasForeignKey(e => e.ParticipantId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Event)
                    .WithMany(e => e.Enrolments)
                    .HasForeignKey(e => e.EventId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Category)
                    .WithMany()
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // RESULTS
            modelBuilder.Entity<Result>(entity =>
            {
                entity.ToTable(t =>
                    t.HasCheckConstraint("CK_Results_Position", "[Position] IS NULL OR [Position] > 0"));

                entity.HasKey(r => r.ResultId);
                entity.HasIndex(r => r.EnrolmentId).IsUnique();
                entity.Property(r => r.ResultStatus).IsRequired().HasMaxLength(30);
                entity.Property(r => r.FinishTime).HasColumnType("time");
                entity.Property(r => r.RecordedAt).HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(r => r.Enrolment)
                    .WithOne(e => e.Result)
                    .HasForeignKey<Result>(r => r.EnrolmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
