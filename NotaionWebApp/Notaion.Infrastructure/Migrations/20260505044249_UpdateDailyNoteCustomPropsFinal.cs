using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notaion.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDailyNoteCustomPropsFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Blur",
                table: "DailyNotes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "BorderStyle",
                table: "DailyNotes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CustomCategory",
                table: "DailyNotes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomColor",
                table: "DailyNotes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomRgb",
                table: "DailyNotes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FontSize",
                table: "DailyNotes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Glow",
                table: "DailyNotes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HideHeader",
                table: "DailyNotes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "DailyNotes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsMinimized",
                table: "DailyNotes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LinkedNoteIds",
                table: "DailyNotes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Opacity",
                table: "DailyNotes",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "Pattern",
                table: "DailyNotes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TitleAlign",
                table: "DailyNotes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Blur",
                table: "DailyNotes");

            migrationBuilder.DropColumn(
                name: "BorderStyle",
                table: "DailyNotes");

            migrationBuilder.DropColumn(
                name: "CustomCategory",
                table: "DailyNotes");

            migrationBuilder.DropColumn(
                name: "CustomColor",
                table: "DailyNotes");

            migrationBuilder.DropColumn(
                name: "CustomRgb",
                table: "DailyNotes");

            migrationBuilder.DropColumn(
                name: "FontSize",
                table: "DailyNotes");

            migrationBuilder.DropColumn(
                name: "Glow",
                table: "DailyNotes");

            migrationBuilder.DropColumn(
                name: "HideHeader",
                table: "DailyNotes");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "DailyNotes");

            migrationBuilder.DropColumn(
                name: "IsMinimized",
                table: "DailyNotes");

            migrationBuilder.DropColumn(
                name: "LinkedNoteIds",
                table: "DailyNotes");

            migrationBuilder.DropColumn(
                name: "Opacity",
                table: "DailyNotes");

            migrationBuilder.DropColumn(
                name: "Pattern",
                table: "DailyNotes");

            migrationBuilder.DropColumn(
                name: "TitleAlign",
                table: "DailyNotes");
        }
    }
}
