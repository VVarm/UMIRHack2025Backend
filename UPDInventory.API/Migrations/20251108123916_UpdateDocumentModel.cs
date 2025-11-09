using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UPDInventory.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDocumentModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_document_items_documents_document_id",
                table: "document_items");

            migrationBuilder.DropForeignKey(
                name: "FK_document_items_products_product_id",
                table: "document_items");

            migrationBuilder.DropForeignKey(
                name: "FK_documents_organizations_organization_id",
                table: "documents");

            migrationBuilder.DropForeignKey(
                name: "FK_documents_users_created_by_id",
                table: "documents");

            migrationBuilder.DropForeignKey(
                name: "FK_documents_warehouses_warehouse_id",
                table: "documents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_documents",
                table: "documents");

            migrationBuilder.DropIndex(
                name: "IX_documents_organization_id_status",
                table: "documents");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Documents_Status",
                table: "documents");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Documents_Type",
                table: "documents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_document_items",
                table: "document_items");

            migrationBuilder.DropColumn(
                name: "date_created",
                table: "documents");

            migrationBuilder.RenameTable(
                name: "documents",
                newName: "Documents");

            migrationBuilder.RenameTable(
                name: "document_items",
                newName: "DocumentItems");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "Documents",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Documents",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "number",
                table: "Documents",
                newName: "Number");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Documents",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "warehouse_id",
                table: "Documents",
                newName: "WarehouseId");

            migrationBuilder.RenameColumn(
                name: "organization_id",
                table: "Documents",
                newName: "OrganizationId");

            migrationBuilder.RenameColumn(
                name: "document_date",
                table: "Documents",
                newName: "DocumentDate");

            migrationBuilder.RenameColumn(
                name: "created_by_id",
                table: "Documents",
                newName: "CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_documents_type",
                table: "Documents",
                newName: "IX_Documents_Type");

            migrationBuilder.RenameIndex(
                name: "IX_documents_warehouse_id",
                table: "Documents",
                newName: "IX_Documents_WarehouseId");

            migrationBuilder.RenameIndex(
                name: "IX_documents_created_by_id",
                table: "Documents",
                newName: "IX_Documents_CreatedByUserId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "DocumentItems",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "quantity_expected",
                table: "DocumentItems",
                newName: "QuantityExpected");

            migrationBuilder.RenameColumn(
                name: "quantity_actual",
                table: "DocumentItems",
                newName: "QuantityActual");

            migrationBuilder.RenameColumn(
                name: "product_id",
                table: "DocumentItems",
                newName: "ProductId");

            migrationBuilder.RenameColumn(
                name: "document_id",
                table: "DocumentItems",
                newName: "DocumentId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "DocumentItems",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_document_items_product_id",
                table: "DocumentItems",
                newName: "IX_DocumentItems_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_document_items_document_id",
                table: "DocumentItems",
                newName: "IX_DocumentItems_DocumentId");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "products",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Documents",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Documents",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "draft",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "WarehouseId",
                table: "Documents",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "Documents",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Documents",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DestinationWarehouseId",
                table: "Documents",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceWarehouseId",
                table: "Documents",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId1",
                table: "Documents",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "QuantityExpected",
                table: "DocumentItems",
                type: "numeric(18,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(12,3)",
                oldPrecision: 12,
                oldScale: 3,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "QuantityActual",
                table: "DocumentItems",
                type: "numeric(18,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(12,3)",
                oldPrecision: 12,
                oldScale: 3,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "DocumentItems",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "NOW()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Documents",
                table: "Documents",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DocumentItems",
                table: "DocumentItems",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DestinationWarehouseId",
                table: "Documents",
                column: "DestinationWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_DocumentDate",
                table: "Documents",
                column: "DocumentDate");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_OrganizationId_Number",
                table: "Documents",
                columns: new[] { "OrganizationId", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documents_SourceWarehouseId",
                table: "Documents",
                column: "SourceWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_Status",
                table: "Documents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_WarehouseId1",
                table: "Documents",
                column: "WarehouseId1");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentItems_Documents_DocumentId",
                table: "DocumentItems",
                column: "DocumentId",
                principalTable: "Documents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentItems_products_ProductId",
                table: "DocumentItems",
                column: "ProductId",
                principalTable: "products",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_organizations_OrganizationId",
                table: "Documents",
                column: "OrganizationId",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_users_CreatedByUserId",
                table: "Documents",
                column: "CreatedByUserId",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_warehouses_DestinationWarehouseId",
                table: "Documents",
                column: "DestinationWarehouseId",
                principalTable: "warehouses",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_warehouses_SourceWarehouseId",
                table: "Documents",
                column: "SourceWarehouseId",
                principalTable: "warehouses",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_warehouses_WarehouseId",
                table: "Documents",
                column: "WarehouseId",
                principalTable: "warehouses",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_warehouses_WarehouseId1",
                table: "Documents",
                column: "WarehouseId1",
                principalTable: "warehouses",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentItems_Documents_DocumentId",
                table: "DocumentItems");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentItems_products_ProductId",
                table: "DocumentItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_organizations_OrganizationId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_users_CreatedByUserId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_warehouses_DestinationWarehouseId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_warehouses_SourceWarehouseId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_warehouses_WarehouseId",
                table: "Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_warehouses_WarehouseId1",
                table: "Documents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Documents",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_DestinationWarehouseId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_DocumentDate",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_OrganizationId_Number",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_SourceWarehouseId",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_Status",
                table: "Documents");

            migrationBuilder.DropIndex(
                name: "IX_Documents_WarehouseId1",
                table: "Documents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DocumentItems",
                table: "DocumentItems");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "products");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "products");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "DestinationWarehouseId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "SourceWarehouseId",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "WarehouseId1",
                table: "Documents");

            migrationBuilder.RenameTable(
                name: "Documents",
                newName: "documents");

            migrationBuilder.RenameTable(
                name: "DocumentItems",
                newName: "document_items");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "documents",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "documents",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Number",
                table: "documents",
                newName: "number");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "documents",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "WarehouseId",
                table: "documents",
                newName: "warehouse_id");

            migrationBuilder.RenameColumn(
                name: "OrganizationId",
                table: "documents",
                newName: "organization_id");

            migrationBuilder.RenameColumn(
                name: "DocumentDate",
                table: "documents",
                newName: "document_date");

            migrationBuilder.RenameColumn(
                name: "CreatedByUserId",
                table: "documents",
                newName: "created_by_id");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_Type",
                table: "documents",
                newName: "IX_documents_type");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_WarehouseId",
                table: "documents",
                newName: "IX_documents_warehouse_id");

            migrationBuilder.RenameIndex(
                name: "IX_Documents_CreatedByUserId",
                table: "documents",
                newName: "IX_documents_created_by_id");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "document_items",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "QuantityExpected",
                table: "document_items",
                newName: "quantity_expected");

            migrationBuilder.RenameColumn(
                name: "QuantityActual",
                table: "document_items",
                newName: "quantity_actual");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "document_items",
                newName: "product_id");

            migrationBuilder.RenameColumn(
                name: "DocumentId",
                table: "document_items",
                newName: "document_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "document_items",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_DocumentItems_ProductId",
                table: "document_items",
                newName: "IX_document_items_product_id");

            migrationBuilder.RenameIndex(
                name: "IX_DocumentItems_DocumentId",
                table: "document_items",
                newName: "IX_document_items_document_id");

            migrationBuilder.AlterColumn<string>(
                name: "type",
                table: "documents",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "documents",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValue: "draft");

            migrationBuilder.AlterColumn<int>(
                name: "warehouse_id",
                table: "documents",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_created",
                table: "documents",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()");

            migrationBuilder.AlterColumn<decimal>(
                name: "quantity_expected",
                table: "document_items",
                type: "numeric(12,3)",
                precision: 12,
                scale: 3,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,3)");

            migrationBuilder.AlterColumn<decimal>(
                name: "quantity_actual",
                table: "document_items",
                type: "numeric(12,3)",
                precision: 12,
                scale: 3,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,3)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "document_items",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddPrimaryKey(
                name: "PK_documents",
                table: "documents",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_document_items",
                table: "document_items",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_documents_organization_id_status",
                table: "documents",
                columns: new[] { "organization_id", "status" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Documents_Status",
                table: "documents",
                sql: "status IN ('draft', 'in_progress', 'completed', 'cancelled')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Documents_Type",
                table: "documents",
                sql: "type IN ('inventory', 'waybill')");

            migrationBuilder.AddForeignKey(
                name: "FK_document_items_documents_document_id",
                table: "document_items",
                column: "document_id",
                principalTable: "documents",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_document_items_products_product_id",
                table: "document_items",
                column: "product_id",
                principalTable: "products",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_documents_organizations_organization_id",
                table: "documents",
                column: "organization_id",
                principalTable: "organizations",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_documents_users_created_by_id",
                table: "documents",
                column: "created_by_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_documents_warehouses_warehouse_id",
                table: "documents",
                column: "warehouse_id",
                principalTable: "warehouses",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
