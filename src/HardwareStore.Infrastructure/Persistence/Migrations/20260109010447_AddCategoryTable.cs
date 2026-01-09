using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HardwareStore.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // PASO 1: Crear tabla Categories
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            // PASO 2: Extraer categorías únicas de Products e insertarlas en Categories
            migrationBuilder.Sql(@"
                INSERT INTO ""Categories"" (""Name"")
                SELECT DISTINCT ""Category""
                FROM ""Products""
                WHERE ""Category"" IS NOT NULL AND ""Category"" != ''
                ORDER BY ""Category"";
            ");

            // PASO 3: Agregar columna CategoryId a Products (nullable temporalmente)
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Products",
                type: "integer",
                nullable: true);

            // PASO 4: Mapear productos existentes a sus CategoryId correspondientes
            migrationBuilder.Sql(@"
                UPDATE ""Products"" p
                SET ""CategoryId"" = c.""Id""
                FROM ""Categories"" c
                WHERE p.""Category"" = c.""Name"";
            ");

            // PASO 5: Hacer CategoryId NOT NULL (ahora que todos los productos tienen CategoryId)
            migrationBuilder.AlterColumn<int>(
                name: "CategoryId",
                table: "Products",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            // PASO 6: Crear índice y foreign key
            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // PASO 7: Eliminar la columna Category antigua
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Products");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Products",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
