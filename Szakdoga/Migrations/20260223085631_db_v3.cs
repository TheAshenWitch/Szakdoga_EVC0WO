using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Szakdoga.Migrations
{
    /// <inheritdoc />
    public partial class db_v3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SheetId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SheetId",
                table: "OrderPieces",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "X",
                table: "OrderPieces",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Y",
                table: "OrderPieces",
                type: "float",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_SheetId",
                table: "Orders",
                column: "SheetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Sheets_SheetId",
                table: "Orders",
                column: "SheetId",
                principalTable: "Sheets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Sheets_SheetId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_SheetId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SheetId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SheetId",
                table: "OrderPieces");

            migrationBuilder.DropColumn(
                name: "X",
                table: "OrderPieces");

            migrationBuilder.DropColumn(
                name: "Y",
                table: "OrderPieces");
        }
    }
}
