using Microsoft.EntityFrameworkCore;

namespace Kviz.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<QuizResult> QuizResults { get; set; }
        public DbSet<UserQuizAttempt> UserQuizAttempts { get; set; }
        public DbSet<Ranking> Rankings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Eksplicitno konfigurisanje identity kolona za Oracle
            modelBuilder.Entity<User>()
                .Property(e => e.User_Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Quiz>()
                .Property(e => e.Quiz_Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Question>()
                .Property(e => e.Question_Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Answer>()
                .Property(e => e.Answer_Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<QuizResult>()
                .Property(e => e.Result_Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<UserQuizAttempt>()
                .Property(e => e.Attempt_Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Ranking>()
                .Property(e => e.Ranking_Id)
                .ValueGeneratedOnAdd();

            // Konfigurisanje relacija
            modelBuilder.Entity<Question>()
                .HasOne(q => q.Quiz)
                .WithMany(quiz => quiz.Questions)
                .HasForeignKey(q => q.Quiz_Id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Answer>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.User_Id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Answer>()
                .HasOne(a => a.Question)
                .WithMany()
                .HasForeignKey(a => a.Question_Id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserQuizAttempt>()
                .HasOne(ua => ua.User)
                .WithMany()
                .HasForeignKey(ua => ua.User_Id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserQuizAttempt>()
                .HasOne(ua => ua.Quiz)
                .WithMany()
                .HasForeignKey(ua => ua.Quiz_Id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuizResult>()
                .HasOne(qr => qr.User)
                .WithMany()
                .HasForeignKey(qr => qr.User_Id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuizResult>()
                .HasOne(qr => qr.Quiz)
                .WithMany()
                .HasForeignKey(qr => qr.Quiz_Id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuizResult>()
                .HasOne(qr => qr.UserQuizAttempt)
                .WithMany()
                .HasForeignKey(qr => qr.Attempt_Id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Ranking>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.User_Id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Ranking>()
                .HasOne(r => r.Quiz)
                .WithMany()
                .HasForeignKey(r => r.Quiz_Id)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}