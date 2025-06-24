using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CardGeneratorBackend.Migrations
{
    /// <inheritdoc />
    public partial class TypeImageFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ImageFileId",
                table: "card_types",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_card_types_ImageFileId",
                table: "card_types",
                column: "ImageFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_card_types_tracked_files_ImageFileId",
                table: "card_types",
                column: "ImageFileId",
                principalTable: "tracked_files",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_card_types_tracked_files_ImageFileId",
                table: "card_types");

            migrationBuilder.DropIndex(
                name: "IX_card_types_ImageFileId",
                table: "card_types");

            migrationBuilder.DropColumn(
                name: "ImageFileId",
                table: "card_types");
        }
    }
}
