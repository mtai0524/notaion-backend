using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notaion.Migrations
{
    /// <inheritdoc />
    public partial class AddFileOwnerIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "FileMetadatas",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FileMetadatas_UserId",
                table: "FileMetadatas",
                column: "UserId");

            // Trước thay đổi này không chỗ nào ghi UserId, nên mọi file cũ đều
            // NULL và sẽ biến mất khỏi trang Files (vốn giờ lọc theo chủ sở hữu).
            // Gán hết cho tài khoản 'minhtai'. Guard EXISTS để DB nào không có tài
            // khoản đó (dev/test mới) thì bỏ qua thay vì ghi NULL đè lên.
            migrationBuilder.Sql(@"
                UPDATE FileMetadatas
                SET UserId = (SELECT TOP 1 Id FROM AspNetUsers WHERE UserName = 'minhtai')
                WHERE UserId IS NULL
                  AND EXISTS (SELECT 1 FROM AspNetUsers WHERE UserName = 'minhtai');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Cố tình không hoàn tác phần backfill: sau khi migration chạy, mọi
            // upload mới cũng ghi UserId, nên xoá ngược sẽ mất luôn quyền sở hữu
            // của những file hợp lệ chứ không chỉ của dữ liệu cũ.
            migrationBuilder.DropIndex(
                name: "IX_FileMetadatas_UserId",
                table: "FileMetadatas");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "FileMetadatas",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
