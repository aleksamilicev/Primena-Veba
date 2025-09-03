using Microsoft.EntityFrameworkCore.Migrations;

namespace Kviz.Migrations
{
    public partial class _20250903184500_InitialCreate: Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // USERS tabela
            migrationBuilder.CreateTable(
                name: "USERS",
                columns: table => new
                {
                    USER_ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    USERNAME = table.Column<string>(maxLength: 255, nullable: false),
                    IS_ADMIN = table.Column<bool>(nullable: false),
                    EMAIL = table.Column<string>(maxLength: 255, nullable: false),
                    PASSWORD_HASH = table.Column<string>(maxLength: 255, nullable: false),
                    PROFILE_IMAGE_URL = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USERS", x => x.USER_ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_USERS_USERNAME",
                table: "USERS",
                column: "USERNAME",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USERS_EMAIL",
                table: "USERS",
                column: "EMAIL",
                unique: true);

            // QUIZZES tabela
            migrationBuilder.CreateTable(
                name: "QUIZZES",
                columns: table => new
                {
                    QUIZ_ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TITLE = table.Column<string>(maxLength: 255, nullable: false),
                    DESCRIPTION = table.Column<string>(type: "text", nullable: true),
                    NUMBER_OF_QUESTIONS = table.Column<int>(nullable: true),
                    CATEGORY = table.Column<string>(maxLength: 255, nullable: true),
                    DIFFICULTY_LEVEL = table.Column<string>(maxLength: 255, nullable: true),
                    TIME_LIMIT = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QUIZZES", x => x.QUIZ_ID);
                });

            // USER_QUIZ_ATTEMPTS tabela
            migrationBuilder.CreateTable(
                name: "USER_QUIZ_ATTEMPTS",
                columns: table => new
                {
                    ATTEMPT_ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    USER_ID = table.Column<int>(nullable: false),
                    QUIZ_ID = table.Column<int>(nullable: false),
                    ATTEMPT_NUMBER = table.Column<int>(nullable: false),
                    ATTEMPT_DATE = table.Column<DateTime>(nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER_QUIZ_ATTEMPTS", x => x.ATTEMPT_ID);
                    table.ForeignKey(
                        name: "FK_USER_QUIZ_ATTEMPTS_USERS_USER_ID",
                        column: x => x.USER_ID,
                        principalTable: "USERS",
                        principalColumn: "USER_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_USER_QUIZ_ATTEMPTS_QUIZZES_QUIZ_ID",
                        column: x => x.QUIZ_ID,
                        principalTable: "QUIZZES",
                        principalColumn: "QUIZ_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            // QUESTIONS tabela
            migrationBuilder.CreateTable(
                name: "QUESTIONS",
                columns: table => new
                {
                    QUESTION_ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QUIZ_ID = table.Column<int>(nullable: false),
                    QUESTION_TEXT = table.Column<string>(type: "text", nullable: false),
                    QUESTION_TYPE = table.Column<string>(maxLength: 255, nullable: true),
                    CORRECT_ANSWER = table.Column<string>(type: "text", nullable: true),
                    DIFFICULTY_LEVEL = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QUESTIONS", x => x.QUESTION_ID);
                    table.ForeignKey(
                        name: "FK_QUESTIONS_QUIZZES_QUIZ_ID",
                        column: x => x.QUIZ_ID,
                        principalTable: "QUIZZES",
                        principalColumn: "QUIZ_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            // ANSWERS tabela
            migrationBuilder.CreateTable(
                name: "ANSWERS",
                columns: table => new
                {
                    ANSWER_ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    USER_ID = table.Column<int>(nullable: false),
                    QUESTION_ID = table.Column<int>(nullable: false),
                    QUIZ_ID = table.Column<int>(nullable: false),
                    ATTEMPT_ID = table.Column<int>(nullable: false),
                    USER_ANSWER = table.Column<string>(type: "text", nullable: true),
                    IS_CORRECT = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ANSWERS", x => x.ANSWER_ID);
                    table.ForeignKey(
                        name: "FK_ANSWERS_USERS_USER_ID",
                        column: x => x.USER_ID,
                        principalTable: "USERS",
                        principalColumn: "USER_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ANSWERS_QUESTIONS_QUESTION_ID",
                        column: x => x.QUESTION_ID,
                        principalTable: "QUESTIONS",
                        principalColumn: "QUESTION_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ANSWERS_QUIZZES_QUIZ_ID",
                        column: x => x.QUIZ_ID,
                        principalTable: "QUIZZES",
                        principalColumn: "QUIZ_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ANSWERS_USER_QUIZ_ATTEMPTS_ATTEMPT_ID",
                        column: x => x.ATTEMPT_ID,
                        principalTable: "USER_QUIZ_ATTEMPTS",
                        principalColumn: "ATTEMPT_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            // QUIZ_RESULTS tabela
            migrationBuilder.CreateTable(
                name: "QUIZ_RESULTS",
                columns: table => new
                {
                    RESULT_ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    USER_ID = table.Column<int>(nullable: false),
                    QUIZ_ID = table.Column<int>(nullable: false),
                    ATTEMPT_ID = table.Column<int>(nullable: false),
                    TOTAL_QUESTIONS = table.Column<int>(nullable: false),
                    CORRECT_ANSWERS = table.Column<int>(nullable: false),
                    SCORE_PERCENTAGE = table.Column<double>(nullable: false),
                    TIME_TAKEN = table.Column<int>(nullable: false),
                    COMPLETED_AT = table.Column<DateTime>(nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QUIZ_RESULTS", x => x.RESULT_ID);
                    table.ForeignKey(
                        name: "FK_QUIZ_RESULTS_USERS_USER_ID",
                        column: x => x.USER_ID,
                        principalTable: "USERS",
                        principalColumn: "USER_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QUIZ_RESULTS_QUIZZES_QUIZ_ID",
                        column: x => x.QUIZ_ID,
                        principalTable: "QUIZZES",
                        principalColumn: "QUIZ_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QUIZ_RESULTS_USER_QUIZ_ATTEMPTS_ATTEMPT_ID",
                        column: x => x.ATTEMPT_ID,
                        principalTable: "USER_QUIZ_ATTEMPTS",
                        principalColumn: "ATTEMPT_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            // RANKING tabela
            migrationBuilder.CreateTable(
                name: "RANKING",
                columns: table => new
                {
                    RANKING_ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QUIZ_ID = table.Column<int>(nullable: false),
                    USER_ID = table.Column<int>(nullable: false),
                    SCORE_PERCENTAGE = table.Column<double>(nullable: false),
                    TIME_TAKEN = table.Column<int>(nullable: false),
                    RANK_POSITION = table.Column<int>(nullable: false),
                    COMPLETED_AT = table.Column<DateTime>(nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RANKING", x => x.RANKING_ID);
                    table.ForeignKey(
                        name: "FK_RANKING_USERS_USER_ID",
                        column: x => x.USER_ID,
                        principalTable: "USERS",
                        principalColumn: "USER_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RANKING_QUIZZES_QUIZ_ID",
                        column: x => x.QUIZ_ID,
                        principalTable: "QUIZZES",
                        principalColumn: "QUIZ_ID",
                        onDelete: ReferentialAction.Cascade);
                });
            }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("ANSWERS");
            migrationBuilder.DropTable("QUIZ_RESULTS");
            migrationBuilder.DropTable("RANKING");
            migrationBuilder.DropTable("QUESTIONS");
            migrationBuilder.DropTable("USER_QUIZ_ATTEMPTS");
            migrationBuilder.DropTable("QUIZZES");
            migrationBuilder.DropTable("USERS");
        }
    }
}
