using Microsoft.EntityFrameworkCore.Migrations;

namespace sellnet.Data.Migrations
{
    public partial class ModifiedSupplier : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Country",
                table: "AspNetUsers",
                newName: "Division");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Division",
                table: "AspNetUsers",
                newName: "Country");
        }
    }
}
