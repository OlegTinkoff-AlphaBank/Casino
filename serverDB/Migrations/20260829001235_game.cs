using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace serverDB.Migrations
{
    /// <inheritdoc />
    public partial class game : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Games");

            migrationBuilder.AddColumn<decimal>(
                name: "GameRate",
                table: "Users",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxCash",
                table: "Promocodes",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GameRate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MaxCash",
                table: "Promocodes");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Games",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");
        }
    }
}
