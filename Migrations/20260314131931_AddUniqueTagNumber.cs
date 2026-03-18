using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwineBreedingManager.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueTagNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Pigs_TagNumber",
                table: "Pigs",
                column: "TagNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Pigs_TagNumber",
                table: "Pigs");
        }
    }
}
