using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notaion.Migrations
{
    /// <inheritdoc />
    public partial class base_entity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Drop primary key constraint
            migrationBuilder.DropPrimaryKey(
                name: "PK_Chat",
                table: "Chat");

            // Step 2: Alter the Id column type to uniqueidentifier (Guid)
            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Chat",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            // Step 3: Re-add primary key constraint
            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat",
                table: "Chat",
                column: "Id");

            // Step 4: Add additional columns
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Chat",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "Chat",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Chat",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "Chat",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove the added columns in the Down method
            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Chat");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Chat");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Chat");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "Chat");

            // Revert the Id column type change
            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Chat",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            // Recreate the primary key constraint
            migrationBuilder.AddPrimaryKey(
                name: "PK_Chat",
                table: "Chat",
                column: "Id");
        }
    }
}
