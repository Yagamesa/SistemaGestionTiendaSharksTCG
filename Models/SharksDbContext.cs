using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ProyectoMDGSharksWeb.Models;

public partial class SharksDbContext : DbContext
{
    public SharksDbContext()
    {
    }

    public SharksDbContext(DbContextOptions<SharksDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Categoria> Categoria { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<CodigoTcg> CodigoTcgs { get; set; }

    public virtual DbSet<Compra> Compras { get; set; }

    public virtual DbSet<CompraProveedor> CompraProveedors { get; set; }

    public virtual DbSet<Deudum> Deuda { get; set; }

    public virtual DbSet<Egreso> Egresos { get; set; }

    public virtual DbSet<GanadoresTorneo> GanadoresTorneos { get; set; }

    public virtual DbSet<Ingreso> Ingresos { get; set; }

    public virtual DbSet<Permiso> Permisos { get; set; } // <-- NUEVO DbSet

    public virtual DbSet<Preventum> Preventa { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<ProductoVenta> ProductoVenta { get; set; }

    public virtual DbSet<Proveedor> Proveedors { get; set; }

    public virtual DbSet<ReporteClientesDestacado> ReporteClientesDestacados { get; set; }

    public virtual DbSet<ReporteProductosMasVendido> ReporteProductosMasVendidos { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<Sharkcoin> Sharkcoins { get; set; }

    public virtual DbSet<Stock> Stocks { get; set; }

    public virtual DbSet<TipoEgreso> TipoEgresos { get; set; }

    public virtual DbSet<TipoIngreso> TipoIngresos { get; set; }

    public virtual DbSet<Torneo> Torneos { get; set; }

    public virtual DbSet<TorneoCliente> TorneoClientes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Venta> Venta { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Categoria>(entity => { entity.HasKey(e => e.Id).HasName("PK__Categori__3213E83FE0D9A6A6"); });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Cliente__3213E83FC3092F26");
            entity.Property(e => e.Deuda).HasDefaultValue(0m);
            entity.Property(e => e.SharkCoins).HasDefaultValue(0m);
        });

        modelBuilder.Entity<CodigoTcg>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__codigo_t__3213E83FC68CB9C0");
            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.CodigoTcgs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__codigo_tc__id_cl__693CA210");
        });

        modelBuilder.Entity<Compra>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Compra__3213E83F706A4B51");
            entity.Property(e => e.FechaCompra).HasDefaultValueSql("(getdate())");
            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Compras)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Compra__id_usuar__3E52440B");
        });

        modelBuilder.Entity<CompraProveedor>(entity =>
        {
            entity.HasKey(e => new { e.IdCompra, e.IdProveedor }).HasName("PK__Compra_P__5C6979E678649434");
            entity.HasOne(d => d.IdCompraNavigation).WithMany(p => p.CompraProveedors)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Compra_Pr__id_co__4222D4EF");
            entity.HasOne(d => d.IdProveedorNavigation).WithMany(p => p.CompraProveedors)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Compra_Pr__id_pr__4316F928");
        });

        modelBuilder.Entity<Deudum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Deuda__3213E83FB3F29878");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.DeudaNavigation)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Deuda__id_client__656C112C");
        });

        modelBuilder.Entity<Egreso>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Egreso__3213E83FE2427F4F");
            entity.Property(e => e.Fecha).HasDefaultValueSql("(getdate())");
            entity.HasOne(d => d.IdTipoEgresoNavigation).WithMany(p => p.Egresos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Egreso__id_tipo___5BE2A6F2");
            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Egresos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Egreso__id_usuar__5CD6CB2B");
        });

        modelBuilder.Entity<GanadoresTorneo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Ganadore__3213E83F3EFB5682");
            entity.Property(e => e.Puesto).HasColumnName("puesto");
            entity.HasOne(d => d.TorneoCliente).WithMany(p => p.GanadoresTorneos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ganadores_Torneo__59063A47");
        });

        modelBuilder.Entity<Ingreso>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Ingreso__3213E83F57946AC0");
            entity.Property(e => e.Fecha).HasDefaultValueSql("(getdate())");
            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Ingresos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ingreso__id_clie__619B8048");
            entity.HasOne(d => d.IdTipoIngresoNavigation).WithMany(p => p.Ingresos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Ingreso__id_tipo__60A75C0F");
        });

        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Modulo).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Accion).IsRequired().HasMaxLength(100);

            entity.HasOne(e => e.Rol)
                .WithMany(r => r.PermisosAsignados)
                .HasForeignKey(e => e.IdRol)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Preventum>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Preventa__3213E83F48C02E7B");
            entity.Property(e => e.FechaPreventa).HasDefaultValueSql("(getdate())");
            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Preventa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Preventa__id_cli__4E88ABD4");
            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Preventa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Preventa__id_usu__4F7CD00D");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Producto__3213E83FB7384401");
            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Productos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Producto__id_cat__36B12243");
        });

        modelBuilder.Entity<ProductoVenta>(entity =>
        {
            entity.HasKey(e => new { e.IdProducto, e.IdVenta }).HasName("PK__Producto__1B6D4F36BF82A541");
            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.ProductoVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Producto___id_pr__4AB81AF0");
            entity.HasOne(d => d.IdVentaNavigation).WithMany(p => p.ProductoVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Producto___id_ve__4BAC3F29");
        });

        modelBuilder.Entity<Proveedor>(entity => { entity.HasKey(e => e.Id).HasName("PK__Proveedo__3213E83F03C48016"); });

        modelBuilder.Entity<ReporteClientesDestacado>(entity => { entity.ToView("ReporteClientesDestacados"); });

        modelBuilder.Entity<ReporteProductosMasVendido>(entity => { entity.ToView("ReporteProductosMasVendidos"); });

        modelBuilder.Entity<Rol>(entity => { entity.HasKey(e => e.Id).HasName("PK__Rol__3213E83F5181D7BF"); });

        modelBuilder.Entity<Sharkcoin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Sharkcoi__3213E83F94044B47");
            entity.Property(e => e.Fecha).HasDefaultValueSql("(getdate())");
            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Sharkcoins)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Sharkcoin__id_cl__6C190EBB");
        });

        modelBuilder.Entity<Stock>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Stock__3213E83F34ADCBF1");
            entity.Property(e => e.FechaMovimiento).HasDefaultValueSql("(getdate())");
            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.Stocks)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Stock__id_produc__398D8EEE");
        });

        modelBuilder.Entity<TipoEgreso>(entity => { entity.HasKey(e => e.Id).HasName("PK__tipo_egr__3213E83F64D053D8"); });

        modelBuilder.Entity<TipoIngreso>(entity => { entity.HasKey(e => e.Id).HasName("PK__tipo_ing__3213E83FA325A08F"); });

        modelBuilder.Entity<Torneo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Torneo__3213E83F7B03CAB9");
            entity.Property(e => e.Entrada).HasColumnName("entrada").HasPrecision(18, 2).HasDefaultValue(0m);
        });

        modelBuilder.Entity<TorneoCliente>(entity =>
        {
            entity.HasKey(e => new { e.IdTorneo, e.IdCliente }).HasName("PK__Torneo_C__9DC1D977D708EADC");
            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.TorneoClientes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Torneo_Cl__id_cl__5629CD9C");
            entity.HasOne(d => d.IdTorneoNavigation).WithMany(p => p.TorneoClientes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Torneo_Cl__id_to__5535A963");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3213E83FABA083BA");
            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Users)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Users__id_rol__2F10007B");
        });

        modelBuilder.Entity<Venta>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Venta__3213E83F95E8804C");
            entity.Property(e => e.FechaVenta).HasDefaultValueSql("(getdate())");
            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Venta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Venta__id_client__45F365D3");
            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Venta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Venta__id_usuari__46E78A0C");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
