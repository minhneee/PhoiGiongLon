using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwineBreedingManager.Migrations
{
    /// <inheritdoc />
    public partial class AddPenManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PenId",
                table: "Pigs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Pens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Capacity = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pens", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pigs_PenId",
                table: "Pigs",
                column: "PenId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pigs_Pens_PenId",
                table: "Pigs",
                column: "PenId",
                principalTable: "Pens",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pigs_Pens_PenId",
                table: "Pigs");

            migrationBuilder.DropTable(
                name: "Pens");

            migrationBuilder.DropIndex(
                name: "IX_Pigs_PenId",
                table: "Pigs");

            migrationBuilder.DropColumn(
                name: "PenId",
                table: "Pigs");
        }
    }
}
