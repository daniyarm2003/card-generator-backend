using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CardGeneratorBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddedQuoteColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "quote",
                table: "cards",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "quote",
                table: "cards");
        }
    }
}
