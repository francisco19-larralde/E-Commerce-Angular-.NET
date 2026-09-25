using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ecommerce.Api.Models;

namespace Ecommerce.Api.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }


    public DbSet<Producto> Productos { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Carrito> Carritos { get; set; }
    public DbSet<CarritoItem> CarritoItems { get; set; }
    public DbSet<ProductoVariante> ProductoVariantes { get; set; }
    public DbSet<Orden> Ordenes { get; set; }
    public DbSet<OrdenItem> OrdenItems { get; set; }
    public DbSet<CuponDescuento> Cupones { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Producto>().Property(p => p.Precio).HasPrecision(18, 2);
        builder.Entity<Producto>().Property(p => p.Nombre).HasMaxLength(150);
        builder.Entity<Producto>().ToTable(t =>
        {
            t.HasCheckConstraint("CK_Productos_Precio", "[Precio] > 0");
            t.HasCheckConstraint("CK_Productos_Stock", "[Stock] >= 0");
        });

        builder.Entity<Categoria>().Property(c => c.Nombre).HasMaxLength(150);
        builder.Entity<Categoria>().HasIndex(c => c.Nombre).IsUnique();

        builder.Entity<Carrito>().HasIndex(c => c.UsuarioId).IsUnique();
        builder.Entity<CarritoItem>()
            .HasIndex(i => new { i.CarritoId, i.ProductoId, i.VarianteId })
            .IsUnique()
            .HasFilter("[VarianteId] IS NOT NULL");
        builder.Entity<CarritoItem>()
            .HasIndex(i => new { i.CarritoId, i.ProductoId })
            .IsUnique()
            .HasFilter("[VarianteId] IS NULL");
        builder.Entity<CarritoItem>().ToTable(t =>
            t.HasCheckConstraint("CK_CarritoItems_Cantidad", "[Cantidad] > 0"));

        builder.Entity<ProductoVariante>().Property(v => v.Talle).HasMaxLength(30);
        builder.Entity<ProductoVariante>()
            .HasIndex(v => new { v.ProductoId, v.Talle })
            .IsUnique();
        builder.Entity<ProductoVariante>().ToTable(t =>
            t.HasCheckConstraint("CK_ProductoVariantes_Stock", "[Stock] >= 0"));

        builder.Entity<CuponDescuento>().Property(c => c.Codigo).HasMaxLength(50);
        builder.Entity<CuponDescuento>().HasIndex(c => c.Codigo).IsUnique();
        builder.Entity<CuponDescuento>().ToTable(t =>
        {
            t.HasCheckConstraint("CK_Cupones_Porcentaje", "[PorcentajeDescuento] BETWEEN 0 AND 100");
            t.HasCheckConstraint("CK_Cupones_UsoMaximo", "[UsoMaximo] IS NULL OR [UsoMaximo] > 0");
            t.HasCheckConstraint("CK_Cupones_VecesUsado", "[VecesUsado] >= 0");
        });

        builder.Entity<Orden>().Property(o => o.Subtotal).HasPrecision(18, 2);
        builder.Entity<Orden>().Property(o => o.Descuento).HasPrecision(18, 2);
        builder.Entity<Orden>().Property(o => o.Total).HasPrecision(18, 2);
        builder.Entity<Orden>().Property(o => o.CuponCodigo).HasMaxLength(50);
        builder.Entity<Orden>().Property(o => o.UltimosDigitosTarjeta).HasMaxLength(4);
        builder.Entity<Orden>().ToTable(t =>
        {
            t.HasCheckConstraint("CK_Ordenes_Subtotal", "[Subtotal] >= 0");
            t.HasCheckConstraint("CK_Ordenes_Descuento", "[Descuento] >= 0");
            t.HasCheckConstraint("CK_Ordenes_Total", "[Total] >= 0");
        });

        builder.Entity<OrdenItem>().Property(i => i.PrecioUnitario).HasPrecision(18, 2);
        builder.Entity<OrdenItem>().Property(i => i.Subtotal).HasPrecision(18, 2);
        builder.Entity<OrdenItem>().Property(i => i.ProductoNombre).HasMaxLength(150);
        builder.Entity<OrdenItem>().Property(i => i.Talle).HasMaxLength(30);
        builder.Entity<OrdenItem>().ToTable(t =>
        {
            t.HasCheckConstraint("CK_OrdenItems_PrecioUnitario", "[PrecioUnitario] >= 0");
            t.HasCheckConstraint("CK_OrdenItems_Cantidad", "[Cantidad] > 0");
            t.HasCheckConstraint("CK_OrdenItems_Subtotal", "[Subtotal] >= 0");
        });
    }

}
