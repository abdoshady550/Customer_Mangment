using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Customer_Mangment.Migrations
{
    /// <inheritdoc />
    public partial class addChangeStateCuloms_HistoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NewData",
                table: "CustomersHistory",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OldData",
                table: "CustomersHistory",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewData",
                table: "CustomersHistory");

            migrationBuilder.DropColumn(
                name: "OldData",
                table: "CustomersHistory");
        }
    }
}
