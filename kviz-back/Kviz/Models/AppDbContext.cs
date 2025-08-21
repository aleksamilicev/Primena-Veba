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

            // ===== Identity kolone =====
            modelBuilder.Entity<User>().Property(u => u.User_Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Quiz>().Property(q => q.Quiz_Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Question>().Property(q => q.Question_Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Answer>().Property(a => a.Answer_Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<UserQuizAttempt>().Property(a => a.Attempt_Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<QuizResult>().Property(r => r.Result_Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<Ranking>().Property(r => r.Ranking_Id).ValueGeneratedOnAdd();

            // ===== User tabela =====
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("USERS");
                entity.HasKey(u => u.User_Id);
            });

            // ===== Quiz tabela =====
            modelBuilder.Entity<Quiz>(entity =>
            {
                entity.ToTable("QUIZZES");
                entity.HasKey(q => q.Quiz_Id);
            });

            // ===== Question tabela =====
            modelBuilder.Entity<Question>(entity =>
            {
                entity.ToTable("QUESTIONS");
                entity.HasKey(q => q.Question_Id);

                entity.Property(q => q.Question_Id).HasColumnName("QUESTION_ID");
                entity.Property(q => q.Quiz_Id).HasColumnName("QUIZ_ID").IsRequired();
                entity.Property(q => q.Question_Text).HasColumnName("QUESTION_TEXT").IsRequired();
                entity.Property(q => q.Question_Type).HasColumnName("QUESTION_TYPE");
                entity.Property(q => q.Correct_Answer).HasColumnName("CORRECT_ANSWER");
                entity.Property(q => q.Difficulty_Level).HasColumnName("DIFFICULTY_LEVEL");

                entity.HasOne(q => q.Quiz)
                      .WithMany(quiz => quiz.Questions)
                      .HasForeignKey(q => q.Quiz_Id)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== UserQuizAttempt tabela =====
            modelBuilder.Entity<UserQuizAttempt>(entity =>
            {
                entity.ToTable("USER_QUIZ_ATTEMPTS");
                entity.HasKey(a => a.Attempt_Id);

                entity.Property(a => a.Attempt_Id).HasColumnName("ATTEMPT_ID");
                entity.Property(a => a.User_Id).HasColumnName("USER_ID").IsRequired();
                entity.Property(a => a.Quiz_Id).HasColumnName("QUIZ_ID").IsRequired();
                entity.Property(a => a.Attempt_Number).HasColumnName("ATTEMPT_NUMBER");
                entity.Property(a => a.Attempt_Date).HasColumnName("ATTEMPT_DATE");

                entity.HasOne(a => a.User)
                      .WithMany(u => u.QuizAttempts)
                      .HasForeignKey(a => a.User_Id)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Quiz)
                      .WithMany(q => q.QuizAttempts)
                      .HasForeignKey(a => a.Quiz_Id)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== Answer tabela =====
            modelBuilder.Entity<Answer>(entity =>
            {
                entity.ToTable("ANSWERS");
                entity.HasKey(a => a.Answer_Id);

                // Eksplicitno mapiranje svake kolone
                entity.Property(a => a.Answer_Id)
                      .HasColumnName("ANSWER_ID")
                      .ValueGeneratedOnAdd();

                entity.Property(a => a.User_Id)
                      .HasColumnName("USER_ID")
                      .IsRequired();

                entity.Property(a => a.Question_Id)
                      .HasColumnName("QUESTION_ID")
                      .IsRequired();

                entity.Property(a => a.Quiz_Id) 
                      .HasColumnName("QUIZ_ID")
                      .IsRequired();

                entity.Property(a => a.Attempt_Id)
                       .HasColumnName("ATTEMPT_ID")
                       .IsRequired();

                entity.Property(a => a.User_Answer)
                      .HasColumnName("USER_ANSWER");

                entity.Property(a => a.Is_Correct)
                      .HasColumnName("IS_CORRECT");

                entity.HasOne(a => a.User)
                      .WithMany(u => u.Answers)
                      .HasForeignKey(a => a.User_Id)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Question)
                      .WithMany(q => q.Answers)
                      .HasForeignKey(a => a.Question_Id)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Quiz)
                      .WithMany(q => q.Answers)
                      .HasForeignKey(a => a.Quiz_Id)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Attempt) // veza na attempt
                      .WithMany(at => at.Answers)   // ili `Answers`, ako hoćeš da preimenuješ kolekciju u modelu
                      .HasForeignKey(a => a.Attempt_Id)
                      .OnDelete(DeleteBehavior.Cascade);

                // UKLANJAMO sve HasOne/WithMany relationship-e za Answer tabelu
                // Ovo sprečava EF da pravi automatske joinove koji prave probleme sa Oracle
            });

            // ===== QuizResult tabela =====
            modelBuilder.Entity<QuizResult>(entity =>
            {
                entity.ToTable("QUIZ_RESULTS");
                entity.HasKey(r => r.Result_Id);

                entity.Property(r => r.Result_Id).HasColumnName("RESULT_ID");
                entity.Property(r => r.User_Id).HasColumnName("USER_ID").IsRequired();
                entity.Property(r => r.Quiz_Id).HasColumnName("QUIZ_ID").IsRequired();
                entity.Property(r => r.Attempt_Id).HasColumnName("ATTEMPT_ID").IsRequired();
                entity.Property(r => r.Score_Percentage).HasColumnName("SCORE_PERCENTAGE");

                entity.HasOne(r => r.User)
                      .WithMany()
                      .HasForeignKey(r => r.User_Id)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Quiz)
                      .WithMany()
                      .HasForeignKey(r => r.Quiz_Id)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.UserQuizAttempt)
                      .WithMany()
                      .HasForeignKey(r => r.Attempt_Id)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== Ranking tabela =====
            modelBuilder.Entity<Ranking>(entity =>
            {
                entity.ToTable("RANKINGS");
                entity.HasKey(r => r.Ranking_Id);

                entity.Property(r => r.Ranking_Id).HasColumnName("RANKING_ID");
                entity.Property(r => r.User_Id).HasColumnName("USER_ID").IsRequired();
                entity.Property(r => r.Quiz_Id).HasColumnName("QUIZ_ID").IsRequired();
                entity.Property(r => r.Rank_Position).HasColumnName("POSITION");

                entity.HasOne(r => r.User)
                      .WithMany()
                      .HasForeignKey(r => r.User_Id)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Quiz)
                      .WithMany()
                      .HasForeignKey(r => r.Quiz_Id)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}