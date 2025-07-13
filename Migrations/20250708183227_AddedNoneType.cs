using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CardGeneratorBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddedNoneType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "card_types",
                columns: new[] { "id", "bg_color_hex_code_1", "bg_color_hex_code_2", "ImageFileId", "name", "text_color" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), "ffffff", "ffffff", null, "None", "000000" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "card_types",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"));
        }
    }
}
