using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Szakdoga.Migrations
{
    /// <inheritdoc />
    public partial class v8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderPieces_Sheets_SheetId",
                table: "OrderPieces");

            migrationBuilder.DropIndex(
                name: "IX_OrderPieces_SheetId",
                table: "OrderPieces");

            migrationBuilder.DropColumn(
                name: "SheetId",
                table: "OrderPieces");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SheetId",
                table: "OrderPieces",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderPieces_SheetId",
                table: "OrderPieces",
                column: "SheetId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderPieces_Sheets_SheetId",
                table: "OrderPieces",
                column: "SheetId",
                principalTable: "Sheets",
                principalColumn: "Id");
        }
    }
}
