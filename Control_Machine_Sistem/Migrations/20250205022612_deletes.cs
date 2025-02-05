using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Control_Machine_Sistem.Migrations
{
    /// <inheritdoc />
    public partial class deletes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Machines_Customers_CustomerId",
                table: "Machines");

            migrationBuilder.DropForeignKey(
                name: "FK_Machines_Models_ModelId",
                table: "Machines");

            migrationBuilder.AddForeignKey(
                name: "FK_Machines_Customers_CustomerId",
                table: "Machines",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Machines_Models_ModelId",
                table: "Machines",
                column: "ModelId",
                principalTable: "Models",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Machines_Customers_CustomerId",
                table: "Machines");

            migrationBuilder.DropForeignKey(
                name: "FK_Machines_Models_ModelId",
                table: "Machines");

            migrationBuilder.AddForeignKey(
                name: "FK_Machines_Customers_CustomerId",
                table: "Machines",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Machines_Models_ModelId",
                table: "Machines",
                column: "ModelId",
                principalTable: "Models",
                principalColumn: "Id");
        }
    }
}
