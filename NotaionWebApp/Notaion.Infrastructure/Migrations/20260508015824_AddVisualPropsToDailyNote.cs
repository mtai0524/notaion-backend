using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notaion.Migrations
{
    /// <inheritdoc />
    public partial class AddVisualPropsToDailyNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "BlurIntensity",
                table: "DailyNotes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "Compact",
                table: "DailyNotes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CustomTextColorHex",
                table: "DailyNotes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FontFamily",
                table: "DailyNotes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "GlowRadius",
                table: "DailyNotes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "Highlighted",
                table: "DailyNotes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "LineHeight",
                table: "DailyNotes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "Locked",
                table: "DailyNotes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Pinned",
                table: "DailyNotes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "Rotation",
                table: "DailyNotes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlurIntensity",
                table: "DailyNotes");

            migrationBuilder.DropColumn(
                name: "Compact",
                table: "DailyNotes");

            migrationBuilder.DropColumn(
                name: "CustomTextColorHex",
                table: "DailyNotes");

            migrationBuilder.DropColumn(
                name: "FontFamily",
                table: "DailyNotes");

            migrationBuilder.DropColumn(
                name: "GlowRadius",
                table: "DailyNotes");

            migrationBuilder.DropColumn(
                name: "Highlighted",
                table: "DailyNotes");

            migrationBuilder.DropColumn(
                name: "LineHeight",
                table: "DailyNotes");

            migrationBuilder.DropColumn(
                name: "Locked",
                table: "DailyNotes");

            migrationBuilder.DropColumn(
                name: "Pinned",
                table: "DailyNotes");

            migrationBuilder.DropColumn(
                name: "Rotation",
                table: "DailyNotes");
        }
    }
}
