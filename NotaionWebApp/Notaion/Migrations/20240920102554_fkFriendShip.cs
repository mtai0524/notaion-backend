using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notaion.Migrations
{
    /// <inheritdoc />
    public partial class fkFriendShip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SenderId",
                table: "FriendShip",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReceiverId",
                table: "FriendShip",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FriendShip_ReceiverId",
                table: "FriendShip",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_FriendShip_SenderId",
                table: "FriendShip",
                column: "SenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_FriendShip_AspNetUsers_ReceiverId",
                table: "FriendShip",
                column: "ReceiverId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FriendShip_AspNetUsers_SenderId",
                table: "FriendShip",
                column: "SenderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FriendShip_AspNetUsers_ReceiverId",
                table: "FriendShip");

            migrationBuilder.DropForeignKey(
                name: "FK_FriendShip_AspNetUsers_SenderId",
                table: "FriendShip");

            migrationBuilder.DropIndex(
                name: "IX_FriendShip_ReceiverId",
                table: "FriendShip");

            migrationBuilder.DropIndex(
                name: "IX_FriendShip_SenderId",
                table: "FriendShip");

            migrationBuilder.AlterColumn<string>(
                name: "SenderId",
                table: "FriendShip",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReceiverId",
                table: "FriendShip",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
