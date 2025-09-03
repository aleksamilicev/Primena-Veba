using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Kviz.Migrations
{
    [DbContext(typeof(QuizDbContext))]
    public class QuizDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.17")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            modelBuilder.Entity("User", b =>
            {
                b.Property<int>("USER_ID").ValueGeneratedOnAdd();
                b.Property<string>("USERNAME").IsRequired().HasMaxLength(255);
                b.Property<string>("EMAIL").IsRequired().HasMaxLength(255);
                b.Property<bool>("IS_ADMIN");
                b.Property<string>("PASSWORD_HASH").IsRequired().HasMaxLength(255);
                b.Property<string>("PROFILE_IMAGE_URL").HasMaxLength(255);
                b.HasKey("USER_ID");
                b.HasIndex("USERNAME").IsUnique();
                b.HasIndex("EMAIL").IsUnique();
                b.ToTable("USERS");
            });

            modelBuilder.Entity("Quiz", b =>
            {
                b.Property<int>("QUIZ_ID").ValueGeneratedOnAdd();
                b.Property<string>("TITLE").IsRequired().HasMaxLength(255);
                b.Property<string>("DESCRIPTION").HasColumnType("text");
                b.Property<int?>("NUMBER_OF_QUESTIONS");
                b.Property<string>("CATEGORY").HasMaxLength(255);
                b.Property<string>("DIFFICULTY_LEVEL").HasMaxLength(255);
                b.Property<int?>("TIME_LIMIT");
                b.HasKey("QUIZ_ID");
                b.ToTable("QUIZZES");
            });
        }
    }
}
