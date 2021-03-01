using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace ChekInsExtension.Database
{
    public partial class CheckInExtensionContext : DbContext
    {
        public CheckInExtensionContext()
        {
        }

        public CheckInExtensionContext(DbContextOptions<CheckInExtensionContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AttendeeType> AttendeeTypes { get; set; }
        public virtual DbSet<CheckIn> CheckIns { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<NoPickupPermission> NoPickupPermissions { get; set; }
        public virtual DbSet<Person> People { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseSqlServer("Server=127.0.0.1,1401;Database=CheckInExtension;User Id=sa;Password=Sherlock69");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CI_AS");

            modelBuilder.Entity<AttendeeType>(entity =>
            {
                entity.ToTable("AttendeeType", "cie");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CheckIn>(entity =>
            {
                entity.ToTable("CheckInUpdateJobs", "cie");

                entity.Property(e => e.SecurityCode)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.HasOne(d => d.AttendeeType)
                    .WithMany(p => p.CheckIns)
                    .HasForeignKey(d => d.AttendeeTypeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CheckIn_AttendeeTypeId");

                entity.HasOne(d => d.Location)
                    .WithMany(p => p.CheckIns)
                    .HasForeignKey(d => d.LocationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CheckIn_LocationId");

                entity.HasOne(d => d.Person)
                    .WithMany(p => p.CheckIns)
                    .HasForeignKey(d => d.PersonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Checkin_PersonId");
            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.ToTable("Location", "cie");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<NoPickupPermission>(entity =>
            {
                entity.ToTable("NoPickupPermission", "cie");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.HasOne(d => d.Person)
                    .WithMany(p => p.NoPickupPermissions)
                    .HasForeignKey(d => d.PersonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_NoPickupPermission_PersonId");
            });

            modelBuilder.Entity<Person>(entity =>
            {
                entity.ToTable("Person", "cie");

                entity.HasIndex(e => e.PeopleId, "UQ_Person_PeopleId")
                    .IsUnique();

                entity.Property(e => e.FistName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
