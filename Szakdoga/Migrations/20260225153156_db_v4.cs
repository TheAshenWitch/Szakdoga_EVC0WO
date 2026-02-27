using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Szakdoga.Migrations
{
    /// <inheritdoc />
    public partial class db_v4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderPieces_Orders_OrderId",
                table: "OrderPieces");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Sheets_SheetId",
                table: "Orders");

            migrationBuilder.AlterColumn<int>(
                name: "SheetId",
                table: "Orders",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "OrderId",
                table: "OrderPieces",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderPieces_Orders_OrderId",
                table: "OrderPieces",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Sheets_SheetId",
                table: "Orders",
                column: "SheetId",
                principalTable: "Sheets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderPieces_Orders_OrderId",
                table: "OrderPieces");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Sheets_SheetId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Orders");

            migrationBuilder.AlterColumn<int>(
                name: "SheetId",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "OrderId",
                table: "OrderPieces",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderPieces_Orders_OrderId",
                table: "OrderPieces",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Sheets_SheetId",
                table: "Orders",
                column: "SheetId",
                principalTable: "Sheets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
