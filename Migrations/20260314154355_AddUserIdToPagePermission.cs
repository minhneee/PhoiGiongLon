using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwineBreedingManager.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToPagePermission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "PagePermissions",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PagePermissions");
        }
    }
}
