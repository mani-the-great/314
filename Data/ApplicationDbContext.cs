using Microsoft.EntityFrameworkCore;
using GolestanSystem.Models;

namespace GolestanSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Professor> Professors { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseClass> CourseClasses { get; set; }
        public DbSet<CourseStudent> CourseStudents { get; set; }
        public DbSet<CourseProfessor> CourseProfessors { get; set; }
        public DbSet<Faculty> Faculties { get; set; }
        public DbSet<Prerequisite> Prerequisites { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(u => u.LastName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasIndex(u => u.Email)
                    .IsUnique();

                entity.Property(u => u.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(u => u.RegisterDate)
                    .HasDefaultValueSql("GETDATE()");

                entity.Property(u => u.PhoneNumber)
                    .HasMaxLength(20);

                entity.Property(u => u.Role)
                    .IsRequired();
            });

            modelBuilder.Entity<Student>(entity =>
            {
                entity.Property(s => s.StudentId)
                    .IsRequired()
                    .HasMaxLength(15);

                entity.HasIndex(s => s.StudentId)
                    .IsUnique();

                entity.Property(s => s.EntryDate)
                    .HasColumnType("date");
            });

            modelBuilder.Entity<Professor>(entity =>
            {
                entity.Property(p => p.ProfessorId)
                    .IsRequired()
                    .HasMaxLength(15);

                entity.HasIndex(p => p.ProfessorId)
                    .IsUnique();

                entity.Property(p => p.Salary)
                    .HasColumnType("decimal(18,2)");

                entity.Property(p => p.HireDate)
                    .HasColumnType("date");
            });

            modelBuilder.Entity<Admin>(entity =>
            {
                entity.Property(a => a.AdminId)
                    .IsRequired()
                    .HasMaxLength(15);

                entity.HasIndex(a => a.AdminId)
                    .IsUnique();
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.Property(c => c.Code)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.HasIndex(c => c.Code)
                    .IsUnique();

                entity.Property(c => c.Title)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(c => c.Credits)
                    .IsRequired();

                entity.Property(c => c.ExamTime)
                    .HasColumnType("datetime");

                entity.HasOne(c => c.Faculty)
                    .WithMany(f => f.Courses)
                    .HasForeignKey(c => c.FacultyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CourseClass>(entity =>
            {
                entity.Property(cc => cc.Building)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(cc => cc.RoomNumber)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(cc => cc.Capacity)
                    .IsRequired();

                entity.Property(cc => cc.StartTime)
                    .HasColumnType("time");

                entity.Property(cc => cc.EndTime)
                    .HasColumnType("time");

                entity.HasIndex(cc => new { cc.Building, cc.RoomNumber, cc.Day, cc.StartTime, cc.EndTime })
                    .IsUnique();

                entity.HasOne(cc => cc.Course)
                    .WithMany(c => c.Classes)
                    .HasForeignKey(cc => cc.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CourseStudent>(entity =>
            {
                entity.HasKey(cs => new { cs.StudentId, cs.CourseClassId });

                entity.Property(cs => cs.Grade)
                    .HasColumnType("decimal(5,2)");

                entity.HasOne(cs => cs.Student)
                    .WithMany(s => s.CourseStudents)
                    .HasForeignKey(cs => cs.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(cs => cs.CourseClass)
                    .WithMany(cc => cc.Students)
                    .HasForeignKey(cs => cs.CourseClassId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CourseProfessor>(entity =>
            {
                entity.HasKey(cp => new { cp.ProfessorId, cp.CourseClassId });

                entity.HasOne(cp => cp.Professor)
                    .WithMany(p => p.CourseProfessors)
                    .HasForeignKey(cp => cp.ProfessorId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(cp => cp.CourseClass)
                    .WithMany(cc => cc.Professors)
                    .HasForeignKey(cp => cp.CourseClassId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Prerequisite>(entity =>
            {
                entity.HasKey(p => new { p.CourseId, p.PrerequisiteCourseId });

                entity.HasOne(p => p.Course)
                    .WithMany(c => c.Prerequisites)
                    .HasForeignKey(p => p.CourseId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.PrerequisiteCourse)
                    .WithMany()
                    .HasForeignKey(p => p.PrerequisiteCourseId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Faculty>(entity =>
            {
                entity.Property(f => f.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasIndex(f => f.Name)
                    .IsUnique();

                entity.Property(f => f.MainBuilding)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(f => f.Budget)
                    .HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<User>()
                .HasDiscriminator<string>("UserType")
                .HasValue<Student>("Student")
                .HasValue<Professor>("Professor")
                .HasValue<Admin>("Admin");

            modelBuilder.Entity<Course>()
                .HasOne(c => c.Faculty)
                .WithMany(f => f.Courses)
                .HasForeignKey(c => c.FacultyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
