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
            : base(options: options)
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
                optionsBuilder.UseSqlServer(connectionString: "Name=ConnectionStrings:Database");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation(annotation: "Relational:Collation", value: "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Adult>(buildAction: entity =>
            {
                entity.HasKey(keyExpression: e => e.PersonId);

                entity.ToTable(name: "Adult", schema: "kt");

                entity.HasIndex(indexExpression: e => e.PersonId, name: "XI_Adult_PersonId")
                    .IsUnique();

                entity.Property(propertyExpression: e => e.PersonId).ValueGeneratedNever();

                entity.Property(propertyExpression: e => e.IsPrimaryContact).HasColumnName(name: "isPrimaryContact");

                entity.Property(propertyExpression: e => e.PhoneNumber)
                    .HasMaxLength(maxLength: 30)
                    .IsUnicode(unicode: false);

                entity.HasOne(navigationExpression: d => d.Person)
                    .WithOne(navigationExpression: p => p.Adult)
                    .HasForeignKey<Adult>(foreignKeyExpression: d => d.PersonId)
                    .OnDelete(deleteBehavior: DeleteBehavior.ClientSetNull)
                    .HasConstraintName(name: "FK_Adult_PersonId");
            });

            modelBuilder.Entity<Attendance>(buildAction: entity =>
            {
                entity.ToTable(name: "Attendance", schema: "kt");

                entity.Property(propertyExpression: e => e.SecurityCode)
                    .IsRequired()
                    .HasMaxLength(maxLength: 10)
                    .IsUnicode(unicode: false);

                entity.HasOne(navigationExpression: d => d.AttendanceType)
                    .WithMany(navigationExpression: p => p.Attendances)
                    .HasForeignKey(foreignKeyExpression: d => d.AttendanceTypeId)
                    .OnDelete(deleteBehavior: DeleteBehavior.ClientSetNull)
                    .HasConstraintName(name: "FK_Attendance_AttendanceTypeId");

                entity.HasOne(navigationExpression: d => d.Location)
                    .WithMany(navigationExpression: p => p.Attendances)
                    .HasForeignKey(foreignKeyExpression: d => d.LocationId)
                    .OnDelete(deleteBehavior: DeleteBehavior.ClientSetNull)
                    .HasConstraintName(name: "FK_Attendance_LocationId");

                entity.HasOne(navigationExpression: d => d.Person)
                    .WithMany(navigationExpression: p => p.Attendances)
                    .HasForeignKey(foreignKeyExpression: d => d.PersonId)
                    .OnDelete(deleteBehavior: DeleteBehavior.ClientSetNull)
                    .HasConstraintName(name: "FK_Attendance_PersonId");
            });

            modelBuilder.Entity<AttendanceType>(buildAction: entity =>
            {
                entity.ToTable(name: "AttendanceType", schema: "kt");

                entity.Property(propertyExpression: e => e.Name)
                    .IsRequired()
                    .HasMaxLength(maxLength: 50)
                    .IsUnicode(unicode: false);
            });

            modelBuilder.Entity<Family>(buildAction: entity =>
            {
                entity.ToTable(name: "Family", schema: "kt");

                entity.HasIndex(indexExpression: e => e.HouseholdId, name: "UQ_Family_HouseholdId")
                    .IsUnique();

                entity.Property(propertyExpression: e => e.Name)
                    .IsRequired()
                    .HasMaxLength(maxLength: 70)
                    .IsUnicode(unicode: false);

                entity.Property(propertyExpression: e => e.UpdateDate).HasDefaultValueSql(sql: "('1980-01-01')");
            });

            modelBuilder.Entity<Kid>(buildAction: entity =>
            {
                entity.HasKey(keyExpression: e => e.PersonId);

                entity.ToTable(name: "Kid", schema: "kt");

                entity.HasIndex(indexExpression: e => e.PersonId, name: "XI_Kid_PersonId")
                    .IsUnique();

                entity.Property(propertyExpression: e => e.PersonId).ValueGeneratedNever();

                entity.Property(propertyExpression: e => e.UpdateDate).HasDefaultValueSql(sql: "('1970-01-01')");

                entity.HasOne(navigationExpression: d => d.Person)
                    .WithOne(navigationExpression: p => p.Kid)
                    .HasForeignKey<Kid>(foreignKeyExpression: d => d.PersonId)
                    .OnDelete(deleteBehavior: DeleteBehavior.ClientSetNull)
                    .HasConstraintName(name: "FK_Kid_PersonId");
            });

            modelBuilder.Entity<Location>(buildAction: entity =>
            {
                entity.ToTable(name: "Location", schema: "kt");

                entity.Property(propertyExpression: e => e.LocationGroupId).HasDefaultValueSql(sql: "((5))");

                entity.Property(propertyExpression: e => e.Name)
                    .IsRequired()
                    .HasMaxLength(maxLength: 50)
                    .IsUnicode(unicode: false);

                entity.HasOne(navigationExpression: d => d.LocationGroup)
                    .WithMany(navigationExpression: p => p.Locations)
                    .HasForeignKey(foreignKeyExpression: d => d.LocationGroupId)
                    .OnDelete(deleteBehavior: DeleteBehavior.ClientSetNull)
                    .HasConstraintName(name: "FK_Location_LocationGroupId");
            });

            modelBuilder.Entity<LocationGroup>(buildAction: entity =>
            {
                entity.ToTable(name: "LocationGroup", schema: "kt");

                entity.Property(propertyExpression: e => e.Name)
                    .IsRequired()
                    .HasMaxLength(maxLength: 50)
                    .IsUnicode(unicode: false);
            });

            modelBuilder.Entity<Person>(buildAction: entity =>
            {
                entity.ToTable(name: "Person", schema: "kt");

                entity.Property(propertyExpression: e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(maxLength: 50)
                    .IsUnicode(unicode: false);

                entity.Property(propertyExpression: e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(maxLength: 50)
                    .IsUnicode(unicode: false);

                entity.HasOne(navigationExpression: d => d.Family)
                    .WithMany(navigationExpression: p => p.People)
                    .HasForeignKey(foreignKeyExpression: d => d.FamilyId)
                    .HasConstraintName(name: "FK_Person_FamilyId");
            });

            modelBuilder.Entity<TaskExecution>(buildAction: entity =>
            {
                entity.ToTable(name: "TaskExecution", schema: "kt");

                entity.Property(propertyExpression: e => e.Environment)
                    .IsRequired()
                    .HasMaxLength(maxLength: 20)
                    .IsUnicode(unicode: false);

                entity.Property(propertyExpression: e => e.TaskName)
                    .IsRequired()
                    .HasMaxLength(maxLength: 50)
                    .IsUnicode(unicode: false)
                    .HasDefaultValueSql(sql: "('UpdateTask')");
            });

            OnModelCreatingPartial(modelBuilder: modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
