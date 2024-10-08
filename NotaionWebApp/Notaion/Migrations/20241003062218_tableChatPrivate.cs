using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notaion.Migrations
{
    /// <inheritdoc />
    public partial class tableChatPrivate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatPrivate",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Sender = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Receiver = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Hide = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatPrivate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatPrivate_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatPrivate_UserId",
                table: "ChatPrivate",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatPrivate");
        }
    }
}
