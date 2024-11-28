using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ParkingApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Drivers",
                columns: table => new
                {
                    IdDriver = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Age = table.Column<int>(type: "integer", nullable: false),
                    CategoryDriverLicence = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drivers", x => x.IdDriver);
                });

            migrationBuilder.CreateTable(
                name: "Buses",
                columns: table => new
                {
                    IdBus = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BusMark = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NumberSign = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TheDriver = table.Column<long>(type: "bigint", nullable: false),
                    NumberOfSeats = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buses", x => x.IdBus);
                    table.ForeignKey(
                        name: "FK_Buses_Drivers_TheDriver",
                        column: x => x.TheDriver,
                        principalTable: "Drivers",
                        principalColumn: "IdDriver",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Buses_NumberSign",
                table: "Buses",
                column: "NumberSign",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Buses_TheDriver",
                table: "Buses",
                column: "TheDriver");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Buses");

            migrationBuilder.DropTable(
                name: "Drivers");
        }
    }
}
