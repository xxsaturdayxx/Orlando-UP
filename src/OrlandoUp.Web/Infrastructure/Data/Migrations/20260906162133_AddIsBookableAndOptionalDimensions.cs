using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrlandoUp.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsBookableAndOptionalDimensions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "WidthIn",
                table: "Products",
                type: "decimal(5,1)",
                precision: 5,
                scale: 1,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,1)",
                oldPrecision: 5,
                oldScale: 1);

            migrationBuilder.AlterColumn<decimal>(
                name: "LengthIn",
                table: "Products",
                type: "decimal(5,1)",
                precision: 5,
                scale: 1,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,1)",
                oldPrecision: 5,
                oldScale: 1);

            migrationBuilder.AddColumn<bool>(
                name: "IsBookable",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Written by hand, and the reason is in Docs/relatorio-leva-02-etapa-1.md. The column
            // is born fail-closed, which is the right rule for every row created from now on; the
            // rows that already exist were all on sale before it existed, so saying nothing about
            // them would silently take seven products off the market. The store default stays off
            // the model on purpose: with one, the provider cannot tell an explicit false from an
            // omission, and the false is the answer that matters here.
            migrationBuilder.Sql("UPDATE [Products] SET [IsBookable] = 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Written by hand, and it is the half of this migration worth reading twice. Going down
            // makes the two dimensions NOT NULL again, and the generated code filled the gap with
            // zero. A product 0 inches wide and 0 inches long reads as INSIDE the 30 by 48 limit,
            // so the rollback would publish "fits the Disney buses" for a machine nobody ever
            // measured — a false claim about a real object, invented by a data migration. Rolling
            // back is allowed exactly while there is nothing to invent, and refuses out loud
            // otherwise, so that a human decides what those products are.
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM [Products] WHERE [WidthIn] IS NULL OR [LengthIn] IS NULL)
    THROW 50000, 'Down would have to invent a width or a length for a product nobody measured. Give those products their dimensions, or remove them, before rolling back.', 1;");

            migrationBuilder.DropColumn(
                name: "IsBookable",
                table: "Products");

            migrationBuilder.AlterColumn<decimal>(
                name: "WidthIn",
                table: "Products",
                type: "decimal(5,1)",
                precision: 5,
                scale: 1,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,1)",
                oldPrecision: 5,
                oldScale: 1,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "LengthIn",
                table: "Products",
                type: "decimal(5,1)",
                precision: 5,
                scale: 1,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,1)",
                oldPrecision: 5,
                oldScale: 1,
                oldNullable: true);
        }
    }
}
