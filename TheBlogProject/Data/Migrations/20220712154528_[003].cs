using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheBlogProject.Migrations
{
    public partial class _003 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MyTags",
                table: "AspNetUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string[]>(
                name: "MyTags",
                table: "AspNetUsers",
                type: "text[]",
                nullable: true);
        }
    }
}
