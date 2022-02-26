﻿using Microsoft.EntityFrameworkCore;

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
        public virtual DbSet<SearchLog> SearchLogs { get; set; }
        public virtual DbSet<SearchLog2Attendance> SearchLog2Attendances { get; set; }
        public virtual DbSet<SearchLog2LocationGroup> SearchLog2LocationGroups { get; set; }
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
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Adult>(entity =>
            {
                entity.HasKey(e => e.PersonId);

                entity.ToTable("Adult", "kt");

                entity.HasIndex(e => e.PersonId, "XI_Adult_PersonId")
                    .IsUnique();

                entity.Property(e => e.PersonId).ValueGeneratedNever();

                entity.Property(e => e.IsPrimaryContact).HasColumnName("isPrimaryContact");

                entity.Property(e => e.PhoneNumber)
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
                entity.ToTable("AttendanceType", "kt");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });
            
            modelBuilder.Entity<Family>(entity =>
            {
                entity.ToTable("Family", "kt");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(70)
                    .IsUnicode(false);

                entity.Property(e => e.UpdateDate).HasDefaultValueSql("('1980-01-01')");
            });

            modelBuilder.Entity<Kid>(entity =>
            {
                entity.HasKey(e => e.PersonId);

                entity.ToTable("Kid", "kt");

                entity.HasIndex(e => e.PersonId, "XI_Kid_PersonId")
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

            modelBuilder.Entity<Person>(entity =>
            {
                entity.ToTable("Person", "kt");

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

            modelBuilder.Entity<SearchLog>(entity =>
            {
                entity.ToTable("SearchLog", "kt");

                entity.Property(e => e.DeviceGuid)
                    .IsRequired()
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.SecurityCode)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SearchLog2Attendance>(entity =>
            {
                entity.ToTable("SearchLog2Attendance", "kt");

                entity.HasOne(d => d.Attendance)
                    .WithMany(p => p.SearchLog2Attendances)
                    .HasForeignKey(d => d.AttendanceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SearchLog2Attendance_AttendanceId");

                entity.HasOne(d => d.SearchLog)
                    .WithMany(p => p.SearchLog2Attendances)
                    .HasForeignKey(d => d.SearchLogId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SearchLog2Attendance_SearchLogId");
            });

            modelBuilder.Entity<SearchLog2LocationGroup>(entity =>
            {
                entity.ToTable("SearchLog2LocationGroup", "kt");

                entity.HasOne(d => d.LocationGroup)
                    .WithMany(p => p.SearchLog2LocationGroups)
                    .HasForeignKey(d => d.LocationGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SearchLog2LocationGroup_LocationGroupId");

                entity.HasOne(d => d.SearchLog)
                    .WithMany(p => p.SearchLog2LocationGroups)
                    .HasForeignKey(d => d.SearchLogId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_SearchLog2LocationGroup_SearchLogId");
            });

            modelBuilder.Entity<TaskExecution>(entity =>
            {
                entity.ToTable("TaskExecution", "kt");

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
