using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

#nullable disable

namespace ChekInsExtension.Database
{
    public partial class CheckInsExtensionContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public CheckInsExtensionContext()
        {
        }

        public CheckInsExtensionContext(DbContextOptions<CheckInsExtensionContext> options, IConfiguration configuration)
            : base(options: options)
        {
            _configuration = configuration;
        }

        public virtual DbSet<Attendance> Attendances { get; set; }
        public virtual DbSet<AttendanceType> AttendanceTypes { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<LocationGroup> LocationGroups { get; set; }
        public virtual DbSet<Person> People { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(connectionString: _configuration.GetConnectionString(name: "Database"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation(annotation: "Relational:Collation", value: "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<Attendance>(buildAction: entity =>
            {
                entity.ToTable(name: "Attendance", schema: "cie");

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
                entity.ToTable(name: "AttendanceType", schema: "cie");

                entity.Property(propertyExpression: e => e.Name)
                    .IsRequired()
                    .HasMaxLength(maxLength: 50)
                    .IsUnicode(unicode: false);
            });

            modelBuilder.Entity<Location>(buildAction: entity =>
            {
                entity.ToTable(name: "Location", schema: "cie");

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
                entity.ToTable(name: "LocationGroup", schema: "cie");

                entity.Property(propertyExpression: e => e.Name)
                    .IsRequired()
                    .HasMaxLength(maxLength: 50)
                    .IsUnicode(unicode: false);
            });

            modelBuilder.Entity<Person>(buildAction: entity =>
            {
                entity.ToTable(name: "Person", schema: "cie");

                entity.HasIndex(indexExpression: e => e.PeopleId, name: "UQ_Person_PeopleId")
                    .IsUnique()
                    .HasFilter(sql: "([PeopleId] IS NOT NULL)");

                entity.Property(propertyExpression: e => e.FistName)
                    .IsRequired()
                    .HasMaxLength(maxLength: 50)
                    .IsUnicode(unicode: false);

                entity.Property(propertyExpression: e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(maxLength: 50)
                    .IsUnicode(unicode: false);
            });

            OnModelCreatingPartial(modelBuilder: modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
