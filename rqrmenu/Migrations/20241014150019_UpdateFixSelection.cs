using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace rqrmenu.Migrations
{
    public partial class UpdateFixSelection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
    name: "Item",
    columns: table => new
    {
        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
        Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
        Image = table.Column<string>(type: "nvarchar(max)", nullable: true),
        Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
        Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false) // Ensure this is defined
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_Item", x => x.Id);
        table.ForeignKey(
            name: "FK_Item_Category_CategoryId",
            column: x => x.CategoryId,
            principalTable: "Category",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    });


            migrationBuilder.CreateTable(
    name: "Orders",
    columns: table => new
    {
        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        CustomerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
        ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
        OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
        Quantity = table.Column<int>(type: "int", nullable: false),
        Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
        TableId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
        TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
        UserId = table.Column<int>(type: "int", nullable: true)
    },
    constraints: table =>
    {
        table.PrimaryKey("PK_Orders", x => x.Id);
        table.ForeignKey(
            name: "FK_Orders_Item_ItemId",
            column: x => x.ItemId,
            principalTable: "Item",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
        table.ForeignKey(
            name: "FK_Orders_Table_TableId",
            column: x => x.TableId,
            principalTable: "Table",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    });

            // Check if the Extra table exists
            migrationBuilder.Sql("IF OBJECT_ID('dbo.Extra', 'U') IS NULL BEGIN RAISERROR('Extra table does not exist.', 16, 1); RETURN; END");

            // Drop the old junction table if it exists
            migrationBuilder.Sql("IF OBJECT_ID('dbo.OrderItemExtras', 'U') IS NOT NULL DROP TABLE dbo.OrderItemExtras;");

            // Create the new junction table
            migrationBuilder.CreateTable(
                name: "OrderItemExtra",
                columns: table => new
                {
                    OrderItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExtraId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItemExtra", x => new { x.OrderItemId, x.ExtraId });
                    table.ForeignKey(
                        name: "FK_OrderItemExtra_Extra_ExtraId",
                        column: x => x.ExtraId,
                        principalTable: "Extra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItemExtra_OrderItem_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemExtra_ExtraId",
                table: "OrderItemExtra",
                column: "ExtraId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "OrderItemExtra");

            migrationBuilder.CreateTable(
                name: "OrderItemExtras",
                columns: table => new
                {
                    OrderItemsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SelectedExtrasId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItemExtras", x => new { x.OrderItemsId, x.SelectedExtrasId });
                    table.ForeignKey(
                        name: "FK_OrderItemExtras_Extra_SelectedExtrasId",
                        column: x => x.SelectedExtrasId,
                        principalTable: "Extra",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItemExtras_OrderItem_OrderItemsId",
                        column: x => x.OrderItemsId,
                        principalTable: "OrderItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemExtras_SelectedExtrasId",
                table: "OrderItemExtras",
                column: "SelectedExtrasId");
        }
    }
}
