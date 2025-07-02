using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CardGeneratorBackend.Migrations
{
    /// <inheritdoc />
    public partial class CreatedCard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cards",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    attack = table.Column<int>(type: "integer", nullable: false),
                    health = table.Column<int>(type: "integer", nullable: false),
                    effect = table.Column<string>(type: "text", nullable: false),
                    type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_image_id = table.Column<Guid>(type: "uuid", nullable: true),
                    card_image_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cards", x => x.id);
                    table.ForeignKey(
                        name: "FK_cards_card_types_type_id",
                        column: x => x.type_id,
                        principalTable: "card_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cards_tracked_files_card_image_id",
                        column: x => x.card_image_id,
                        principalTable: "tracked_files",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_cards_tracked_files_display_image_id",
                        column: x => x.display_image_id,
                        principalTable: "tracked_files",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_cards_card_image_id",
                table: "cards",
                column: "card_image_id");

            migrationBuilder.CreateIndex(
                name: "IX_cards_display_image_id",
                table: "cards",
                column: "display_image_id");

            migrationBuilder.CreateIndex(
                name: "IX_cards_type_id",
                table: "cards",
                column: "type_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cards");
        }
    }
}
