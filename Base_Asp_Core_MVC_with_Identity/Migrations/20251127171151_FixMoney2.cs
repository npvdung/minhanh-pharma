using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Base_Asp_Core_MVC_with_Identity.Migrations
{
    public partial class FixMoney2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                table: "AspNetUsers",
                type: "datetime(6)",
                nullable: true,
                defaultValue: new DateTime(2025, 11, 28, 0, 11, 51, 678, DateTimeKind.Local).AddTicks(2070),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true,
                oldDefaultValue: new DateTime(2025, 11, 27, 23, 35, 35, 679, DateTimeKind.Local).AddTicks(8450));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                table: "AspNetUsers",
                type: "datetime(6)",
                nullable: true,
                defaultValue: new DateTime(2025, 11, 27, 23, 35, 35, 679, DateTimeKind.Local).AddTicks(8450),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldNullable: true,
                oldDefaultValue: new DateTime(2025, 11, 28, 0, 11, 51, 678, DateTimeKind.Local).AddTicks(2070));
        }
    }
}
