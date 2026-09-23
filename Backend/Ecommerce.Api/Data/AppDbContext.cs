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
        builder.Entity<Orden>().Property(o => o.Subtotal).HasPrecision(18, 2);
        builder.Entity<Orden>().Property(o => o.Descuento).HasPrecision(18, 2);
        builder.Entity<Orden>().Property(o => o.Total).HasPrecision(18, 2);
        builder.Entity<OrdenItem>().Property(i => i.PrecioUnitario).HasPrecision(18, 2);
        builder.Entity<OrdenItem>().Property(i => i.Subtotal).HasPrecision(18, 2);
    }

}
