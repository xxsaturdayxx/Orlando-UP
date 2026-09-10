using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrlandoUp.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBatteriesAndOperationalSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Batteries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    AssetTag = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    RangeMiles = table.Column<decimal>(type: "decimal(5,1)", precision: 5, scale: 1, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    PurchasedOn = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Batteries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Batteries_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OperationalSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    ChargerCount = table.Column<int>(type: "int", nullable: false),
                    SecondBatteryPerDay = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    LostChargerFee = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationalSettings", x => x.Id);
                });

            // Written by hand, and it is the only statement of this migration that carries data.
            // A settings table with no row is a null reference waiting on the first screen that
            // reads it, and the screen is forbidden to create one (D4/04b, control C03): so the
            // migration that makes the table also makes its single row, id 1, once.
            //
            // The three values are the operation as it stands on 2026-09-10. ChargerCount 14 and
            // LostChargerFee 30 are D37 verbatim. SecondBatteryPerDay 8.00 is the middle of the
            // US$ 5 to US$ 10 range D37 records as "about US$ 8" — the spec left the number
            // unstated, and it is the one value here that is a default rather than a fact. All
            // three are editable on the settings screen, which is the point of the table.
            //
            // UpdatedAtUtc stays null on purpose: nobody has edited this row, and a date invented
            // here would say somebody had.
            migrationBuilder.InsertData(
                table: "OperationalSettings",
                columns: new[] { "Id", "ChargerCount", "SecondBatteryPerDay", "LostChargerFee", "UpdatedAtUtc" },
                values: new object[] { 1, 14, 8.00m, 30.00m, null });

            migrationBuilder.CreateIndex(
                name: "IX_Batteries_AssetTag",
                table: "Batteries",
                column: "AssetTag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Batteries_ProductId",
                table: "Batteries",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Going down loses the twelve batteries, and that is why they are seeded by a command
            // the operator runs rather than by this migration: fleet data that a rollback destroys
            // should not be data a rollback was told to own. The settings row goes with its table,
            // which is the same statement read twice.
            migrationBuilder.DropTable(
                name: "Batteries");

            migrationBuilder.DropTable(
                name: "OperationalSettings");
        }
    }
}
