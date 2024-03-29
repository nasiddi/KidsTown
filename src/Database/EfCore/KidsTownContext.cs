﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

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

        public virtual DbSet<Adult> Adults { get; set; } = null!;
        public virtual DbSet<Attendance> Attendances { get; set; } = null!;
        public virtual DbSet<AttendanceType> AttendanceTypes { get; set; } = null!;
        public virtual DbSet<DocElement> DocElements { get; set; } = null!;
        public virtual DbSet<DocImage> DocImages { get; set; } = null!;
        public virtual DbSet<DocParagraph> DocParagraphs { get; set; } = null!;
        public virtual DbSet<DocTitle> DocTitles { get; set; } = null!;
        public virtual DbSet<Family> Families { get; set; } = null!;
        public virtual DbSet<Kid> Kids { get; set; } = null!;
        public virtual DbSet<Location> Locations { get; set; } = null!;
        public virtual DbSet<LocationGroup> LocationGroups { get; set; } = null!;
        public virtual DbSet<Person> People { get; set; } = null!;
        public virtual DbSet<SearchLog> SearchLogs { get; set; } = null!;
        public virtual DbSet<SearchLog2Attendance> SearchLog2Attendances { get; set; } = null!;
        public virtual DbSet<SearchLog2LocationGroup> SearchLog2LocationGroups { get; set; } = null!;
        public virtual DbSet<TaskExecution> TaskExecutions { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Name=ConnectionStrings:Database");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<DocElement>(entity =>
            {
                entity.ToTable("DocElement", "kt");

                entity.HasIndex(e => e.PreviousId, "UQ_DocElement_PreviousId")
                    .IsUnique()
                    .HasFilter("([PreviousId] IS NOT NULL)");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.Previous)
                    .WithOne(p => p.InversePrevious)
                    .HasForeignKey<DocElement>(d => d.PreviousId)
                    .HasConstraintName("FK_DocElement_PreviousId");
            });

            modelBuilder.Entity<DocImage>(entity =>
            {
                entity.ToTable("DocImage", "kt");

                entity.HasIndex(e => e.PreviousId, "UQ_DocImage_PreviousId")
                    .IsUnique()
                    .HasFilter("([PreviousId] IS NOT NULL)");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.FileId).HasMaxLength(100);

                entity.HasOne(d => d.Element)
                    .WithMany(p => p.DocImages)
                    .HasForeignKey(d => d.ElementId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DocImage_ElementId");

                entity.HasOne(d => d.Previous)
                    .WithOne(p => p.InversePrevious)
                    .HasForeignKey<DocImage>(d => d.PreviousId)
                    .HasConstraintName("FK_DocImage_PreviousId");
            });

            modelBuilder.Entity<DocParagraph>(entity =>
            {
                entity.ToTable("DocParagraph", "kt");

                entity.HasIndex(e => e.PreviousId, "UQ_DocParagraph_PreviousId")
                    .IsUnique()
                    .HasFilter("([PreviousId] IS NOT NULL)");

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.Content).HasMaxLength(512);

                entity.HasOne(d => d.Element)
                    .WithMany(p => p.DocParagraphs)
                    .HasForeignKey(d => d.ElementId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DocParagraph_ElementId");

                entity.HasOne(d => d.Previous)
                    .WithOne(p => p.InversePrevious)
                    .HasForeignKey<DocParagraph>(d => d.PreviousId)
                    .HasConstraintName("FK_DocParagraph_PreviousId");
            });

            modelBuilder.Entity<DocTitle>(entity =>
            {
                entity.ToTable("DocTitle", "kt");

                entity.HasIndex(e => e.ElementId, "UQ_DocTitle_ElementId")
                    .IsUnique();

                entity.Property(e => e.Content).HasMaxLength(100);

                entity.HasOne(d => d.Element)
                    .WithOne(p => p.DocTitle)
                    .HasForeignKey<DocTitle>(d => d.ElementId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DocTitle_ElementId");
            });

            modelBuilder.Entity<Family>(entity =>
            {
                entity.ToTable("Family", "kt");

                entity.Property(e => e.Name)
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
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Person>(entity =>
            {
                entity.ToTable("Person", "kt");

                entity.Property(e => e.FirstName)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.LastName)
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
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.SecurityCode)
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
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.TaskName)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasDefaultValueSql("('UpdateTask')");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
