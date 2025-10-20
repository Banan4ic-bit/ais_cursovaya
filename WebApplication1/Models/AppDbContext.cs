using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Models
{
    public partial class AppDbContext : DbContext
    {
        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Course> Courses { get; set; }
        public virtual DbSet<CourseOverview> CourseOverviews { get; set; }
        public virtual DbSet<CoursePriceChange> CoursePriceChanges { get; set; }
        public virtual DbSet<CourseScheduleView> CourseScheduleViews { get; set; }
        public virtual DbSet<Organization> Organizations { get; set; }
        public virtual DbSet<RequestParticipant> RequestParticipants { get; set; }
        public virtual DbSet<Teacher> Teachers { get; set; }
        public virtual DbSet<TeacherAssignment> TeacherAssignments { get; set; }
        public virtual DbSet<TrainingRequest> TrainingRequests { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning Вынеси строку подключения в appsettings.json для безопасности
            => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=courses;Username=postgres;Password=Nevada");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ---- COURSES ----
            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(e => e.CourseId).HasName("courses_pkey");
                entity.Property(e => e.CourseId).ValueGeneratedOnAdd(); // ✅ Автоинкремент
            });

            modelBuilder.Entity<CourseOverview>(entity =>
            {
                entity.ToView("course_overview");
            });

            // ---- COURSE PRICE CHANGES ----
            modelBuilder.Entity<CoursePriceChange>(entity =>
            {
                entity.HasKey(e => e.DocId).HasName("course_price_changes_pkey");
                entity.Property(e => e.DocId).ValueGeneratedOnAdd(); // ✅
                entity.HasOne(d => d.Course).WithMany(p => p.CoursePriceChanges)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("course_price_changes_course_id_fkey");
            });

            // ---- ORGANIZATIONS ----
            modelBuilder.Entity<Organization>(entity =>
            {
                entity.HasKey(e => e.OrgId).HasName("organizations_pkey");
                entity.Property(e => e.OrgId).ValueGeneratedOnAdd(); // ✅
            });

            // ---- REQUEST PARTICIPANTS ----
            modelBuilder.Entity<RequestParticipant>(entity =>
            {
                entity.HasKey(e => e.ParticipantId).HasName("request_participants_pkey");
                entity.Property(e => e.ParticipantId).ValueGeneratedOnAdd(); // ✅
                entity.HasOne(d => d.Request).WithMany(p => p.RequestParticipants)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("request_participants_request_id_fkey");
            });

            // ---- TEACHERS ----
            modelBuilder.Entity<Teacher>(entity =>
            {
                entity.HasKey(e => e.TeacherId).HasName("teachers_pkey");
                entity.Property(e => e.TeacherId).ValueGeneratedOnAdd(); // ✅
            });

            // ---- TEACHER ASSIGNMENTS ----
            modelBuilder.Entity<TeacherAssignment>(entity =>
            {
                entity.HasKey(e => e.AssignmentId).HasName("teacher_assignments_pkey");
                entity.Property(e => e.AssignmentId).ValueGeneratedOnAdd(); // ✅
                entity.HasOne(d => d.Course).WithMany(p => p.TeacherAssignments)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("teacher_assignments_course_id_fkey");
                entity.HasOne(d => d.Teacher).WithMany(p => p.TeacherAssignments)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("teacher_assignments_teacher_id_fkey");
            });

            // ---- TRAINING REQUESTS ----
            modelBuilder.Entity<TrainingRequest>(entity =>
            {
                entity.HasKey(e => e.RequestId).HasName("training_requests_pkey");
                entity.Property(e => e.RequestId).ValueGeneratedOnAdd();

                entity.HasOne(d => d.Course).WithMany(p => p.TrainingRequests)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("training_requests_course_id_fkey");

                entity.HasOne(d => d.Org).WithMany(p => p.TrainingRequests)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("training_requests_org_id_fkey");

                entity.HasOne(d => d.Assignment).WithMany(p => p.TrainingRequests)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("training_requests_assignment_id_fkey");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
