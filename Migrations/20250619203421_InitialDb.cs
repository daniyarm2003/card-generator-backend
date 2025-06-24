using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CardGeneratorBackend.Migrations
{
    /// <inheritdoc />
    public partial class InitialDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "card_types",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    bg_color_hex_code_1 = table.Column<string>(type: "text", nullable: false),
                    bg_color_hex_code_2 = table.Column<string>(type: "text", nullable: false),
                    text_color = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_card_types", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "card_types");
        }
    }
}
