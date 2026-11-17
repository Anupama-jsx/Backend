using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddRacePersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Passages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Passages", x => x.Id);
                    table.CheckConstraint("CK_Passages_Content", "LEN(LTRIM(RTRIM([Content]))) > 0");
                });

            migrationBuilder.CreateTable(
                name: "Races",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PassageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    StartsAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationSeconds = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Races", x => x.Id);
                    table.CheckConstraint("CK_Races_DurationSeconds", "[DurationSeconds] > 0");
                    table.CheckConstraint("CK_Races_Status", "[Status] IN ('Waiting', 'Countdown', 'Running', 'Completed', 'Cancelled')");
                    table.CheckConstraint("CK_Races_Timing", "[EndedAtUtc] IS NULL OR [StartsAtUtc] IS NULL OR [EndedAtUtc] >= [StartsAtUtc]");
                    table.ForeignKey(
                        name: "FK_Races_Passages_PassageId",
                        column: x => x.PassageId,
                        principalTable: "Passages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RaceParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RaceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    JoinedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    FinishedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ElapsedMilliseconds = table.Column<long>(type: "bigint", nullable: true),
                    Wpm = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    AccuracyPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    TotalCharacterAttempts = table.Column<int>(type: "int", nullable: true),
                    CorrectCharacterAttempts = table.Column<int>(type: "int", nullable: true),
                    FinalCorrectCharacters = table.Column<int>(type: "int", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RaceParticipants", x => x.Id);
                    table.CheckConstraint("CK_RaceParticipants_Accuracy", "[AccuracyPercentage] IS NULL OR [AccuracyPercentage] BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_RaceParticipants_Attempts", "([TotalCharacterAttempts] IS NULL OR [TotalCharacterAttempts] >= 0) AND ([CorrectCharacterAttempts] IS NULL OR [CorrectCharacterAttempts] >= 0) AND ([CorrectCharacterAttempts] IS NULL OR [TotalCharacterAttempts] IS NULL OR [CorrectCharacterAttempts] <= [TotalCharacterAttempts])");
                    table.CheckConstraint("CK_RaceParticipants_Elapsed", "[ElapsedMilliseconds] IS NULL OR [ElapsedMilliseconds] >= 0");
                    table.CheckConstraint("CK_RaceParticipants_FinalCorrectCharacters", "[FinalCorrectCharacters] IS NULL OR [FinalCorrectCharacters] >= 0");
                    table.CheckConstraint("CK_RaceParticipants_Status", "[Status] IN ('Joined', 'Finished', 'TimedOut', 'Left')");
                    table.CheckConstraint("CK_RaceParticipants_Wpm", "[Wpm] IS NULL OR [Wpm] >= 0");
                    table.ForeignKey(
                        name: "FK_RaceParticipants_Races_RaceId",
                        column: x => x.RaceId,
                        principalTable: "Races",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RaceParticipants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RaceParticipants_RaceId_UserId",
                table: "RaceParticipants",
                columns: new[] { "RaceId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RaceParticipants_UserId",
                table: "RaceParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Races_PassageId",
                table: "Races",
                column: "PassageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RaceParticipants");

            migrationBuilder.DropTable(
                name: "Races");

            migrationBuilder.DropTable(
                name: "Passages");
        }
    }
}
