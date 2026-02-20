using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Customer_Mangment.Migrations
{
    /// <inheritdoc />
    public partial class apply_softDelete_onCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomersHistory_Customers_CustomerId",
                table: "CustomersHistory");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Customers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomersHistory_Customers_CustomerId",
                table: "CustomersHistory",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomersHistory_Customers_CustomerId",
                table: "CustomersHistory");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Customers");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomersHistory_Customers_CustomerId",
                table: "CustomersHistory",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
