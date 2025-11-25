using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Base_Asp_Core_MVC_with_Identity.Migrations
{
    public partial class AddImportIdToReturnProductDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImportId",
                table: "Return_Product_Details",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                table: "AspNetUsers",
                type: "datetime(6)",
                nullable: true,
                defaultValue: new DateTime(2025, 11, 24, 13, 33, 47, 52, DateTimeKind.Local).AddTicks(6860),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true,
                oldDefaultValue: new DateTime(2025, 11, 23, 23, 18, 4, 170, DateTimeKind.Local).AddTicks(3580));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImportId",
                table: "Return_Product_Details");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                table: "AspNetUsers",
                type: "datetime(6)",
                nullable: true,
                defaultValue: new DateTime(2025, 11, 23, 23, 18, 4, 170, DateTimeKind.Local).AddTicks(3580),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true,
                oldDefaultValue: new DateTime(2025, 11, 24, 13, 33, 47, 52, DateTimeKind.Local).AddTicks(6860));
        }
    }
}
