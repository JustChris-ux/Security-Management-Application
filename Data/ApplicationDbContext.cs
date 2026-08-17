using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SecurityFirm.Models;

namespace SecurityFirm.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Location> Locations { get; set; }
        public DbSet<SecurityCamera> SecurityCameras { get; set; }
        public DbSet<StaffMember> StaffMembers { get; set; }
        public DbSet<StaffAssignment> StaffAssignments { get; set; }
        public DbSet<ScheduleEntry> ScheduleEntries { get; set; }
        public DbSet<LocationDocument> LocationDocuments { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Location
            builder.Entity<Location>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
                entity.Property(e => e.RiskLevel).HasConversion<int>();
            });

            // SecurityCamera
            builder.Entity<SecurityCamera>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Location)
                      .WithMany(l => l.Cameras)
                      .HasForeignKey(e => e.LocationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // StaffMember
            builder.Entity<StaffMember>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.HourlyRate).HasColumnType("decimal(10,2)");
            });

            // StaffAssignment
            builder.Entity<StaffAssignment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.StaffMember)
                      .WithMany(s => s.Assignments)
                      .HasForeignKey(e => e.StaffMemberId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Location)
                      .WithMany(l => l.Assignments)
                      .HasForeignKey(e => e.LocationId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ScheduleEntry
            builder.Entity<ScheduleEntry>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.HoursWorked).HasColumnType("decimal(5,2)");
                entity.Property(e => e.ShiftType).HasConversion<int>();
                entity.HasIndex(e => new { e.StaffMemberId, e.LocationId, e.Date }).IsUnique();
                entity.HasOne(e => e.StaffMember)
                      .WithMany(s => s.ScheduleEntries)
                      .HasForeignKey(e => e.StaffMemberId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Location)
                      .WithMany(l => l.ScheduleEntries)
                      .HasForeignKey(e => e.LocationId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // LocationDocument
            builder.Entity<LocationDocument>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Location)
                      .WithMany(l => l.Documents)
                      .HasForeignKey(e => e.LocationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ActivityLog
            builder.Entity<ActivityLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.CreatedAt);
            });
        }
    }
}
