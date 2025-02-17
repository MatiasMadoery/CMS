using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Control_Machine_Sistem.Migrations
{
    /// <inheritdoc />
    public partial class NewVariables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocUrls",
                table: "Machines",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocUrls",
                table: "Machines");
        }
    }
}
