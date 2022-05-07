using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    public partial class ModelsChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Locations_Signs_SignId",
                table: "Locations");

            migrationBuilder.DropForeignKey(
                name: "FK_Locations_Waypoints_WaypointId",
                table: "Locations");

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_Signs_SignId",
                table: "Locations",
                column: "SignId",
                principalTable: "Signs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_Waypoints_WaypointId",
                table: "Locations",
                column: "WaypointId",
                principalTable: "Waypoints",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Locations_Signs_SignId",
                table: "Locations");

            migrationBuilder.DropForeignKey(
                name: "FK_Locations_Waypoints_WaypointId",
                table: "Locations");

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_Signs_SignId",
                table: "Locations",
                column: "SignId",
                principalTable: "Signs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_Waypoints_WaypointId",
                table: "Locations",
                column: "WaypointId",
                principalTable: "Waypoints",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
