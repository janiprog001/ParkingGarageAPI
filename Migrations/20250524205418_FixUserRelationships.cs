using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParkingGarageAPI.Migrations
{
    /// <inheritdoc />
    public partial class FixUserRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ParkingHistories_UserId",
                table: "ParkingHistories",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ParkingHistories_Users_UserId",
                table: "ParkingHistories",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ParkingHistories_Users_UserId",
                table: "ParkingHistories");

            migrationBuilder.DropIndex(
                name: "IX_ParkingHistories_UserId",
                table: "ParkingHistories");
        }
    }
}
