using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WebApplication1.Migrations
{
    public partial class v40 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "AvailableAccounts");

            migrationBuilder.AlterColumn<string>(
                name: "SubCategory",
                table: "AvailableAccounts",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "AvailableAccounts",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "AvailableAccounts",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "AvailableAccounts",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<byte>(
                name: "SubCategory",
                table: "AvailableAccounts",
                nullable: false);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "AvailableAccounts",
                nullable: false);

            migrationBuilder.AlterColumn<byte>(
                name: "Category",
                table: "AvailableAccounts",
                nullable: false);
        }
    }
}
