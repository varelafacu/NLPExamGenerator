using Microsoft.EntityFrameworkCore;
using NLPExamGenerator.Entidades;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace NLPExamGenerator.Entidades.EF
{
    public class NLPExamGeneratorContext : DbContext
    {
        public NLPExamGeneratorContext() { }

        public NLPExamGeneratorContext(DbContextOptions<NLPExamGeneratorContext> options) : base(options) { }

        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Exam> Exam { get; set; }
        public DbSet<ExamQuestion> ExamQuestion { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=PNLExamGenerator;Trusted_Connection=True;TrustServerCertificate=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Password).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Nombre).HasMaxLength(200);

                // Relación con Exams
                entity.HasMany(e => e.Exams)
                      .WithOne(e => e.Usuario)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Exam>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(500).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(2000);
                entity.Property(e => e.SourceText).IsRequired();
                entity.Property(e => e.SourceSummary).HasMaxLength(2000);
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UserId).IsRequired();
                
                // Relación con ExamQuestions
                entity.HasMany(e => e.Questions)
                      .WithOne(e => e.Exam)
                      .HasForeignKey(e => e.ExamId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ExamQuestion>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Question).IsRequired();
                entity.Property(e => e.Options).IsRequired(); // JSON string
                entity.Property(e => e.CorrectIndex).IsRequired();
                entity.Property(e => e.Explanation).HasMaxLength(1000);
                entity.Property(e => e.ExamId).IsRequired();
            });
        }
    }
}

