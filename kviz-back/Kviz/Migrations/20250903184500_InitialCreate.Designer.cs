using Kviz.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Kviz.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20250903184500_InitialCreate")]
    partial class _20250903184500_InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.17")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            modelBuilder.Entity("User", b =>
            {
                b.Property<int>("USER_ID")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int");

                b.Property<string>("USERNAME")
                    .IsRequired()
                    .HasMaxLength(255);

                b.Property<string>("EMAIL")
                    .IsRequired()
                    .HasMaxLength(255);

                b.Property<bool>("IS_ADMIN")
                    .HasColumnType("bit");

                b.Property<string>("PASSWORD_HASH")
                    .IsRequired()
                    .HasMaxLength(255);

                b.Property<string>("PROFILE_IMAGE_URL")
                    .HasMaxLength(255);

                b.HasKey("USER_ID");

                b.HasIndex("USERNAME").IsUnique();
                b.HasIndex("EMAIL").IsUnique();

                b.ToTable("USERS");
            });

            modelBuilder.Entity("Quiz", b =>
            {
                b.Property<int>("QUIZ_ID")
                    .ValueGeneratedOnAdd();

                b.Property<string>("TITLE")
                    .IsRequired()
                    .HasMaxLength(255);

                b.Property<string>("DESCRIPTION")
                    .HasColumnType("text");

                b.Property<int?>("NUMBER_OF_QUESTIONS");

                b.Property<string>("CATEGORY")
                    .HasMaxLength(255);

                b.Property<string>("DIFFICULTY_LEVEL")
                    .HasMaxLength(255);

                b.Property<int?>("TIME_LIMIT");

                b.HasKey("QUIZ_ID");

                b.ToTable("QUIZZES");
            });

            modelBuilder.Entity("Question", b =>
            {
                b.Property<int>("QUESTION_ID")
                    .ValueGeneratedOnAdd();

                b.Property<int>("QUIZ_ID");

                b.Property<string>("QUESTION_TEXT")
                    .IsRequired()
                    .HasColumnType("text");

                b.Property<string>("QUESTION_TYPE")
                    .HasMaxLength(255);

                b.Property<string>("CORRECT_ANSWER")
                    .HasColumnType("text");

                b.Property<string>("DIFFICULTY_LEVEL")
                    .HasMaxLength(255);

                b.HasKey("QUESTION_ID");

                b.HasOne("Quiz")
                    .WithMany()
                    .HasForeignKey("QUIZ_ID")
                    .OnDelete(DeleteBehavior.Cascade);

                b.ToTable("QUESTIONS");
            });

            modelBuilder.Entity("Answer", b =>
            {
                b.Property<int>("ANSWER_ID")
                    .ValueGeneratedOnAdd();

                b.Property<int>("USER_ID");
                b.Property<int>("QUESTION_ID");
                b.Property<int>("QUIZ_ID");
                b.Property<int?>("ATTEMPT_ID");
                b.Property<string>("USER_ANSWER").HasColumnType("text");
                b.Property<bool>("IS_CORRECT");

                b.HasKey("ANSWER_ID");

                b.HasOne("User")
                    .WithMany()
                    .HasForeignKey("USER_ID")
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne("Question")
                    .WithMany()
                    .HasForeignKey("QUESTION_ID")
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne("Quiz")
                    .WithMany()
                    .HasForeignKey("QUIZ_ID")
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne("UserQuizAttempt")
                    .WithMany()
                    .HasForeignKey("ATTEMPT_ID")
                    .OnDelete(DeleteBehavior.Cascade);

                b.ToTable("ANSWERS");
            });

            modelBuilder.Entity("UserQuizAttempt", b =>
            {
                b.Property<int>("ATTEMPT_ID")
                    .ValueGeneratedOnAdd();

                b.Property<int>("USER_ID");
                b.Property<int>("QUIZ_ID");
                b.Property<int?>("ATTEMPT_NUMBER");
                b.Property<DateTime?>("ATTEMPT_DATE");

                b.HasKey("ATTEMPT_ID");

                b.HasOne("User")
                    .WithMany()
                    .HasForeignKey("USER_ID")
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne("Quiz")
                    .WithMany()
                    .HasForeignKey("QUIZ_ID")
                    .OnDelete(DeleteBehavior.Cascade);

                b.ToTable("USER_QUIZ_ATTEMPTS");
            });

            modelBuilder.Entity("QuizResult", b =>
            {
                b.Property<int>("RESULT_ID")
                    .ValueGeneratedOnAdd();

                b.Property<int>("USER_ID");
                b.Property<int>("QUIZ_ID");
                b.Property<int?>("ATTEMPT_ID");
                b.Property<int?>("TOTAL_QUESTIONS");
                b.Property<int?>("CORRECT_ANSWERS");
                b.Property<float?>("SCORE_PERCENTAGE");
                b.Property<int?>("TIME_TAKEN");
                b.Property<DateTime?>("COMPLETED_AT");

                b.HasKey("RESULT_ID");

                b.HasOne("User")
                    .WithMany()
                    .HasForeignKey("USER_ID")
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne("Quiz")
                    .WithMany()
                    .HasForeignKey("QUIZ_ID")
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne("UserQuizAttempt")
                    .WithMany()
                    .HasForeignKey("ATTEMPT_ID")
                    .OnDelete(DeleteBehavior.Cascade);

                b.ToTable("QUIZ_RESULTS");
            });

            modelBuilder.Entity("Ranking", b =>
            {
                b.Property<int>("RANKING_ID")
                    .ValueGeneratedOnAdd();

                b.Property<int>("QUIZ_ID");
                b.Property<int>("USER_ID");
                b.Property<float?>("SCORE_PERCENTAGE");
                b.Property<int?>("TIME_TAKEN");
                b.Property<int?>("RANK_POSITION");
                b.Property<DateTime?>("COMPLETED_AT");

                b.HasKey("RANKING_ID");

                b.HasOne("Quiz")
                    .WithMany()
                    .HasForeignKey("QUIZ_ID")
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne("User")
                    .WithMany()
                    .HasForeignKey("USER_ID")
                    .OnDelete(DeleteBehavior.Cascade);

                b.ToTable("RANKING");
            });
        }
    }
}
