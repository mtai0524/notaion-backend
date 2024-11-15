using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notaion.Migrations
{
    /// <inheritdoc />
    public partial class addColIsNewChatPrivate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsNew",
                table: "ChatPrivate",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsNew",
                table: "ChatPrivate");
        }
    }
}
