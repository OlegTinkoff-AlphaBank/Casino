using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace serverDB.Migrations
{
    /// <inheritdoc />
    public partial class fixGameRounds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GameRounds_IdempotencyKey",
                table: "GameRounds");

            migrationBuilder.CreateIndex(
                name: "IX_GameRounds_IdempotencyKey",
                table: "GameRounds",
                column: "IdempotencyKey");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GameRounds_IdempotencyKey",
                table: "GameRounds");

            migrationBuilder.CreateIndex(
                name: "IX_GameRounds_IdempotencyKey",
                table: "GameRounds",
                column: "IdempotencyKey",
                unique: true);
        }
    }
}
