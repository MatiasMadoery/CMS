using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Control_Machine_Sistem.Migrations
{
    /// <inheritdoc />
    public partial class changes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Manual",
                table: "Models",
                newName: "ManualUrls");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ManualUrls",
                table: "Models",
                newName: "Manual");
        }
    }
}
