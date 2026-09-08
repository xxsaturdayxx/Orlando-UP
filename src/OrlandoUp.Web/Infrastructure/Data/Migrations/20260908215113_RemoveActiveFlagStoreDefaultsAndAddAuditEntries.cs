using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrlandoUp.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveActiveFlagStoreDefaultsAndAddAuditEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Products",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "DeliveryZones",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "DeliveryLocations",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "AddOns",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            // Written by hand, and the reason is in Docs/relatorio-leva-04-etapa-1.md §7 and in the
            // note EMENDA-04-03 C1. This is the FIFTH boolean store default, and the generator
            // cannot see it: leva 02 added Products.IsBookable with a defaultValue so that the seven
            // rows that already existed would say what they meant, SQL Server turned that into a
            // permanent constraint, and the model snapshot never carried it — so the model-to-model
            // diff has nothing to remove and no later migration would ever remove it. D34 is about
            // the column, not about the snapshot, and a decision that holds in the model while
            // failing in the database is worse than one that fails in both, because it reads green.
            // The constraint's name is server-generated and differs between databases, which is why
            // this is the same dynamic form the generator emits for the other four. The variable is
            // named rather than numbered because the whole script is one batch and the generated
            // blocks own @var through @var3.
            migrationBuilder.Sql(@"
                DECLARE @bookableDefault nvarchar(max);
                SELECT @bookableDefault = QUOTENAME([d].[name])
                FROM [sys].[default_constraints] [d]
                INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id]
                                              AND [d].[parent_object_id] = [c].[object_id]
                WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Products]') AND [c].[name] = N'IsBookable');
                IF @bookableDefault IS NOT NULL EXEC(N'ALTER TABLE [Products] DROP CONSTRAINT ' + @bookableDefault + ';');");

            migrationBuilder.CreateTable(
                name: "AuditEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActorEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<int>(type: "int", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntries_OccurredAtUtc",
                table: "AuditEntries",
                column: "OccurredAtUtc",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditEntries");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "DeliveryZones",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "DeliveryLocations",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "AddOns",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            // The mirror of the hand-written block of the Up, and it is here so that going down
            // lands on the state this migration found rather than on a third state no migration
            // describes. CAST(0 AS bit) and not 1: the constraint leva 02 created says false, which
            // is the fail-closed answer for a product nobody decided about (D32). Restoring a
            // default cannot invent a value for a row that already has one, so this direction is as
            // honest as the other.
            migrationBuilder.Sql(
                "ALTER TABLE [Products] ADD DEFAULT CAST(0 AS bit) FOR [IsBookable];");
        }
    }
}
