using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomCreation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "HostUserId",
                table: "Races",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoomCode",
                table: "Races",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            // Preserve existing races and give each a distinct legacy room code.
            migrationBuilder.Sql("""
                IF (SELECT COUNT_BIG(*) FROM [Races]) > 99999999
                    THROW 50001, 'Too many legacy races for eight-character codes.', 1;
                WITH Numbered AS (
                    SELECT [RoomCode], ROW_NUMBER() OVER (ORDER BY [Id]) AS Number
                    FROM [Races]
                )
                UPDATE Numbered SET [RoomCode] = RIGHT('00000000' + CAST(Number AS varchar(8)), 8);
                """);

            migrationBuilder.InsertData(
                table: "Passages",
                columns: new[] { "Id", "Content", "CreatedAtUtc" },
                values: new object[] { new Guid("118f5ad7-31de-4713-93df-c26a6d683012"), "Small steps lead to steady progress. Take your time, find your rhythm, and focus on each word. Every race is another chance to learn and improve.", new DateTime(2026, 11, 17, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.CreateIndex(
                name: "IX_Races_HostUserId",
                table: "Races",
                column: "HostUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Races_RoomCode",
                table: "Races",
                column: "RoomCode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Races_Users_HostUserId",
                table: "Races",
                column: "HostUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Races_Users_HostUserId",
                table: "Races");

            migrationBuilder.DropIndex(
                name: "IX_Races_HostUserId",
                table: "Races");

            migrationBuilder.DropIndex(
                name: "IX_Races_RoomCode",
                table: "Races");

            migrationBuilder.DeleteData(
                table: "Passages",
                keyColumn: "Id",
                keyValue: new Guid("118f5ad7-31de-4713-93df-c26a6d683012"));

            migrationBuilder.DropColumn(
                name: "HostUserId",
                table: "Races");

            migrationBuilder.DropColumn(
                name: "RoomCode",
                table: "Races");
        }
    }
}
