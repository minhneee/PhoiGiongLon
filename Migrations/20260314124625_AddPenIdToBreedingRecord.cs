using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwineBreedingManager.Migrations
{
    /// <inheritdoc />
    public partial class AddPenIdToBreedingRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SaleRecords_PigId",
                table: "SaleRecords");

            migrationBuilder.AddColumn<int>(
                name: "PenId",
                table: "BreedingRecords",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaleRecords_PigId",
                table: "SaleRecords",
                column: "PigId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BreedingRecords_PenId",
                table: "BreedingRecords",
                column: "PenId");

            migrationBuilder.AddForeignKey(
                name: "FK_BreedingRecords_Pens_PenId",
                table: "BreedingRecords",
                column: "PenId",
                principalTable: "Pens",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BreedingRecords_Pens_PenId",
                table: "BreedingRecords");

            migrationBuilder.DropIndex(
                name: "IX_SaleRecords_PigId",
                table: "SaleRecords");

            migrationBuilder.DropIndex(
                name: "IX_BreedingRecords_PenId",
                table: "BreedingRecords");

            migrationBuilder.DropColumn(
                name: "PenId",
                table: "BreedingRecords");

            migrationBuilder.CreateIndex(
                name: "IX_SaleRecords_PigId",
                table: "SaleRecords",
                column: "PigId");
        }
    }
}
