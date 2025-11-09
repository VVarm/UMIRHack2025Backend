using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPDInventory.API.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerRoleToUserOrganizations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_warehouses_WarehouseId1",
                table: "Documents");

            migrationBuilder.DropCheckConstraint(
                name: "CK_UserOrganizations_Role",
                table: "user_organizations");

            migrationBuilder.DropIndex(
                name: "IX_Documents_WarehouseId1",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "WarehouseId1",
                table: "Documents");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserOrganizations_Role",
                table: "user_organizations",
                sql: "role IN ('storekeeper', 'auditor', 'admin', 'owner')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_UserOrganizations_Role",
                table: "user_organizations");

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId1",
                table: "Documents",
                type: "integer",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserOrganizations_Role",
                table: "user_organizations",
                sql: "role IN ('storekeeper', 'auditor', 'admin')");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_WarehouseId1",
                table: "Documents",
                column: "WarehouseId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_warehouses_WarehouseId1",
                table: "Documents",
                column: "WarehouseId1",
                principalTable: "warehouses",
                principalColumn: "id");
        }
    }
}
