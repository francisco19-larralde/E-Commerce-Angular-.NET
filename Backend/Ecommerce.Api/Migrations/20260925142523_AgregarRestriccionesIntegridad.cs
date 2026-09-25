using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRestriccionesIntegridad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductoVariantes_ProductoId",
                table: "ProductoVariantes");

            migrationBuilder.DropIndex(
                name: "IX_Carritos_UsuarioId",
                table: "Carritos");

            migrationBuilder.DropIndex(
                name: "IX_CarritoItems_CarritoId",
                table: "CarritoItems");

            migrationBuilder.AlterColumn<string>(
                name: "Talle",
                table: "ProductoVariantes",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Productos",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Talle",
                table: "OrdenItems",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProductoNombre",
                table: "OrdenItems",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "UltimosDigitosTarjeta",
                table: "Ordenes",
                type: "nvarchar(4)",
                maxLength: 4,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CuponCodigo",
                table: "Ordenes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "Cupones",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Categorias",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_ProductoVariantes_ProductoId_Talle",
                table: "ProductoVariantes",
                columns: new[] { "ProductoId", "Talle" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProductoVariantes_Stock",
                table: "ProductoVariantes",
                sql: "[Stock] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Productos_Precio",
                table: "Productos",
                sql: "[Precio] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Productos_Stock",
                table: "Productos",
                sql: "[Stock] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrdenItems_Cantidad",
                table: "OrdenItems",
                sql: "[Cantidad] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrdenItems_PrecioUnitario",
                table: "OrdenItems",
                sql: "[PrecioUnitario] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_OrdenItems_Subtotal",
                table: "OrdenItems",
                sql: "[Subtotal] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Ordenes_Descuento",
                table: "Ordenes",
                sql: "[Descuento] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Ordenes_Subtotal",
                table: "Ordenes",
                sql: "[Subtotal] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Ordenes_Total",
                table: "Ordenes",
                sql: "[Total] >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_Cupones_Codigo",
                table: "Cupones",
                column: "Codigo",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Cupones_Porcentaje",
                table: "Cupones",
                sql: "[PorcentajeDescuento] BETWEEN 0 AND 100");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Cupones_UsoMaximo",
                table: "Cupones",
                sql: "[UsoMaximo] IS NULL OR [UsoMaximo] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Cupones_VecesUsado",
                table: "Cupones",
                sql: "[VecesUsado] >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_Nombre",
                table: "Categorias",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Carritos_UsuarioId",
                table: "Carritos",
                column: "UsuarioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CarritoItems_CarritoId_ProductoId",
                table: "CarritoItems",
                columns: new[] { "CarritoId", "ProductoId" },
                unique: true,
                filter: "[VarianteId] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CarritoItems_CarritoId_ProductoId_VarianteId",
                table: "CarritoItems",
                columns: new[] { "CarritoId", "ProductoId", "VarianteId" },
                unique: true,
                filter: "[VarianteId] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CarritoItems_Cantidad",
                table: "CarritoItems",
                sql: "[Cantidad] > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductoVariantes_ProductoId_Talle",
                table: "ProductoVariantes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ProductoVariantes_Stock",
                table: "ProductoVariantes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Productos_Precio",
                table: "Productos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Productos_Stock",
                table: "Productos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrdenItems_Cantidad",
                table: "OrdenItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrdenItems_PrecioUnitario",
                table: "OrdenItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_OrdenItems_Subtotal",
                table: "OrdenItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Ordenes_Descuento",
                table: "Ordenes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Ordenes_Subtotal",
                table: "Ordenes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Ordenes_Total",
                table: "Ordenes");

            migrationBuilder.DropIndex(
                name: "IX_Cupones_Codigo",
                table: "Cupones");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Cupones_Porcentaje",
                table: "Cupones");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Cupones_UsoMaximo",
                table: "Cupones");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Cupones_VecesUsado",
                table: "Cupones");

            migrationBuilder.DropIndex(
                name: "IX_Categorias_Nombre",
                table: "Categorias");

            migrationBuilder.DropIndex(
                name: "IX_Carritos_UsuarioId",
                table: "Carritos");

            migrationBuilder.DropIndex(
                name: "IX_CarritoItems_CarritoId_ProductoId",
                table: "CarritoItems");

            migrationBuilder.DropIndex(
                name: "IX_CarritoItems_CarritoId_ProductoId_VarianteId",
                table: "CarritoItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CarritoItems_Cantidad",
                table: "CarritoItems");

            migrationBuilder.AlterColumn<string>(
                name: "Talle",
                table: "ProductoVariantes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Productos",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Talle",
                table: "OrdenItems",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProductoNombre",
                table: "OrdenItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "UltimosDigitosTarjeta",
                table: "Ordenes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4)",
                oldMaxLength: 4,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CuponCodigo",
                table: "Ordenes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "Cupones",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Categorias",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.CreateIndex(
                name: "IX_ProductoVariantes_ProductoId",
                table: "ProductoVariantes",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Carritos_UsuarioId",
                table: "Carritos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_CarritoItems_CarritoId",
                table: "CarritoItems",
                column: "CarritoId");
        }
    }
}
