using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Control_Machine_Sistem.Migrations
{
    /// <inheritdoc />
    public partial class Secondmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MachineId1",
                table: "Services",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManualPath",
                table: "Machines",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ServiceManualPath",
                table: "Machines",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Services_MachineId1",
                table: "Services",
                column: "MachineId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Services_Machines_MachineId1",
                table: "Services",
                column: "MachineId1",
                principalTable: "Machines",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Services_Machines_MachineId1",
                table: "Services");

            migrationBuilder.DropIndex(
                name: "IX_Services_MachineId1",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "MachineId1",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "ManualPath",
                table: "Machines");

            migrationBuilder.DropColumn(
                name: "ServiceManualPath",
                table: "Machines");
        }
    }
}
