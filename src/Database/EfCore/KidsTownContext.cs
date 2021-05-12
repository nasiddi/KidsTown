using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace KidsTown.Database.EfCore
{
    public partial class KidsTownContext : DbContext
    {
        public KidsTownContext()
        {
        }

        public KidsTownContext(DbContextOptions<KidsTownContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Adult> Adults { get; set; }
        public virtual DbSet<Attendance> Attendances { get; set; }
        public virtual DbSet<AttendanceType> AttendanceTypes { get; set; }
        public virtual DbSet<Family> Families { get; set; }
        public virtual DbSet<Kid> Kids { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<LocationGroup> LocationGroups { get; set; }
        public virtual DbSet<Person> People { get; set; }
        public virtual DbSet<TaskExecution> TaskExecutions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Name=ConnectionStrings:Database");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation(annotation: "Relational:Collation", value: "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Adult>(entity =>
            {
                entity.HasKey(e => e.PersonId);

                entity.ToTable(name: "Adult", schema: "kt");

                entity.HasIndex(indexExpression: e => e.PersonId, name: "XI_Adult_PersonId")
                    .IsUnique();

                entity.Property(e => e.PersonId).ValueGeneratedNever();

                entity.Property(e => e.IsPrimaryContact).HasColumnName("isPrimaryContact");

                entity.Property(e => e.PhoneNumber)
                    .IsRequired()
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.HasOne(d => d.Person)
                    .WithOne(p => p.Adult)
                    .HasForeignKey<Adult>(d => d.PersonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Adult_PersonId");
            });

            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.ToTable(name: "Attendance", schema: "kt");

                entity.Property(e => e.SecurityCode)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.HasOne(d => d.AttendanceType)
                    .WithMany(p => p.Attendances)
                    .HasForeignKey(d => d.AttendanceTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Attendance_AttendanceTypeId");

                entity.HasOne(d => d.Location)
                    .WithMany(p => p.Attendances)
                    .HasForeignKey(d => d.LocationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Attendance_LocationId");

                entity.HasOne(d => d.Person)
                    .WithMany(p => p.Attendances)
                    .HasForeignKey(d => d.PersonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Attendance_PersonId");
            });

            modelBuilder.Entity<AttendanceType>(entity =>
            {
                entity.ToTable(name: "AttendanceType", schema: "kt");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Family>(entity =>
            {
                entity.ToTable(name: "Family", schema: "kt");

                entity.HasIndex(indexExpression: e => e.HouseholdId, name: "UQ_Family_HouseholdId")
                    .IsUnique();

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(70)
                    .IsUnicode(false);

                entity.Property(e => e.UpdateDate).HasDefaultValueSql("('1980-01-01')");
            });

            modelBuilder.Entity<Kid>(entity =>
            {
                entity.HasKey(e => e.PersonId);

                entity.ToTable(name: "Kid", schema: "kt");

                entity.HasIndex(indexExpression: e => e.PersonId, name: "XI_Kid_PersonId")
                    .IsUnique();

                entity.Property(e => e.PersonId).ValueGeneratedNever();

                entity.Property(e => e.UpdateDate).HasDefaultValueSql("('1970-01-01')");

                entity.HasOne(d => d.Person)
                    .WithOne(p => p.Kid)
                    .HasForeignKey<Kid>(d => d.PersonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Kid_PersonId");
            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.ToTable(name: "Location", schema: "kt");

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
                entity.ToTable(name: "LocationGroup", schema: "kt");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Person>(entity =>
            {
                entity.ToTable(name: "Person", schema: "kt");

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Family)
                    .WithMany(p => p.People)
                    .HasForeignKey(d => d.FamilyId)
                    .HasConstraintName("FK_Person_FamilyId");
            });

            modelBuilder.Entity<TaskExecution>(entity =>
            {
                entity.ToTable(name: "TaskExecution", schema: "kt");

                entity.Property(e => e.Environment)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.TaskName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('UpdateTask')");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
