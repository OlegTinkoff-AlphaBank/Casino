using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace serverDB.Migrations
{
    /// <inheritdoc />
    public partial class ActicsController : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "GameRate",
                table: "Games",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GameRate",
                table: "Games");
        }
    }
}
