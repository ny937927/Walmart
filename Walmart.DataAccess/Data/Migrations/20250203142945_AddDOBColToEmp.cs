using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WalmartWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDOBColToEmp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "DOB",
                table: "Employees",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DOB",
                table: "Employees");
        }
    }
}
