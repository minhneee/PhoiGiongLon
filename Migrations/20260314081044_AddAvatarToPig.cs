using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwineBreedingManager.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarToPig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Pigs",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Pigs");
        }
    }
}
