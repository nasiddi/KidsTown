using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

#nullable disable

namespace KidsTown.Database
{
    public partial class KidsTownContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public KidsTownContext()
        {
        }

        public KidsTownContext(DbContextOptions<KidsTownContext> options, IConfiguration configuration)
            : base(options: options)
        {
            _configuration = configuration;
        }

        public virtual DbSet<Adult> Adults { get; set; }
        public virtual DbSet<Attendance> Attendances { get; set; }
        public virtual DbSet<AttendanceType> AttendanceTypes { get; set; }
        public virtual DbSet<Family> Families { get; set; }
        public virtual DbSet<Kid> Kids { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<LocationGroup> LocationGroups { get; set; }
        public virtual DbSet<TaskExecution> TaskExecutions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(connectionString: _configuration.GetConnectionString(name: "Database"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Adult>(entity =>
            {
                entity.ToTable("Adult", "kt");

                entity.HasIndex(e => e.PeopleId, "UQ_Adult_PeopleId")
                    .IsUnique();

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.PhoneNumber)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.HasOne(d => d.Family)
                    .WithMany(p => p.Adults)
                    .HasForeignKey(d => d.FamilyId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Adult_FamilyId");
            });

            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.ToTable("Attendance", "kt");

                entity.Property(e => e.SecurityCode)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.HasOne(d => d.AttendanceType)
                    .WithMany(p => p.Attendances)
                    .HasForeignKey(d => d.AttendanceTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Attendance_AttendanceTypeId");

                entity.HasOne(d => d.Kid)
                    .WithMany(p => p.Attendances)
                    .HasForeignKey(d => d.KidId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Attendance_KidId");

                entity.HasOne(d => d.Location)
                    .WithMany(p => p.Attendances)
                    .HasForeignKey(d => d.LocationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Attendance_LocationId");
            });

            modelBuilder.Entity<AttendanceType>(entity =>
            {
                entity.ToTable("AttendanceType", "kt");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Family>(entity =>
            {
                entity.ToTable("Family", "kt");

                entity.HasIndex(e => e.HouseholdId, "UQ_Family_HouseholdId")
                    .IsUnique();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(70)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Kid>(entity =>
            {
                entity.ToTable("Kid", "kt");

                entity.HasIndex(e => e.PeopleId, "UQ_Kid_PeopleId")
                    .IsUnique()
                    .HasFilter("([PeopleId] IS NOT NULL)");

                entity.Property(e => e.FistName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Family)
                    .WithMany(p => p.Kids)
                    .HasForeignKey(d => d.FamilyId)
                    .HasConstraintName("FK_Kid_FamilyId");
            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.ToTable("Location", "kt");

                entity.Property(e => e.LocationGroupId).HasDefaultValueSql("((5))");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.LocationGroup)
                    .WithMany(p => p.Locations)
                    .HasForeignKey(d => d.LocationGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Location_LocationGroupId");
            });

            modelBuilder.Entity<LocationGroup>(entity =>
            {
                entity.ToTable("LocationGroup", "kt");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<TaskExecution>(entity =>
            {
                entity.ToTable("TaskExecution", "kt");

                entity.Property(e => e.Environment)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
