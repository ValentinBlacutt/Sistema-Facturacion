using Microsoft.EntityFrameworkCore;
using sistemaFacturacion.Models;

namespace sistemaFacturacion.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Producto> Productos { get; set; }
        public DbSet<Factura> Facturas { get; set; }
        public DbSet<DetalleFactura> DetalleFacturas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración del índice único para código de barras
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.HasIndex(p => p.CodigoBarras)
                      .IsUnique()
                      .HasFilter("[CodigoBarras] IS NOT NULL");

                entity.Property(p => p.CodigoBarras)
                      .HasMaxLength(20);
            });

            // Configuración de relaciones
            modelBuilder.Entity<Factura>()
                .HasMany(f => f.Detalles)
                .WithOne(df => df.Factura)
                .HasForeignKey(df => df.FacturaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Producto>()
                .HasMany<DetalleFactura>()
                .WithOne(df => df.Producto)
                .HasForeignKey(df => df.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de valores por defecto
            modelBuilder.Entity<Producto>()
                .Property(p => p.AlertaStockMinimo)
                .HasDefaultValue(5);

            modelBuilder.Entity<Producto>()
                .Property(p => p.EstaEliminado)
                .HasDefaultValue(false);
        }
    }
}