using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Base_Asp_Core_MVC_with_Identity.Migrations
{
    public partial class RemoveImportName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ImportName",
                table: "ImportsProduct",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                table: "AspNetUsers",
                type: "datetime(6)",
                nullable: true,
                defaultValue: new DateTime(2025, 12, 1, 11, 11, 50, 358, DateTimeKind.Local).AddTicks(8400),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true,
                oldDefaultValue: new DateTime(2025, 11, 28, 0, 11, 51, 678, DateTimeKind.Local).AddTicks(2070));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "ImportsProduct",
                keyColumn: "ImportName",
                keyValue: null,
                column: "ImportName",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "ImportName",
                table: "ImportsProduct",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                table: "AspNetUsers",
                type: "datetime(6)",
                nullable: true,
                defaultValue: new DateTime(2025, 11, 28, 0, 11, 51, 678, DateTimeKind.Local).AddTicks(2070),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true,
                oldDefaultValue: new DateTime(2025, 12, 1, 11, 11, 50, 358, DateTimeKind.Local).AddTicks(8400));
        }
    }
}
