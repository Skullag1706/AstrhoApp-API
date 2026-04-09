using System;
using System.Collections.Generic;
using AstrhoApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AstrhoApp.API.Data;

public partial class AstrhoAppDbContext : DbContext
{
    public AstrhoAppDbContext()
    {
    }

    public AstrhoAppDbContext(DbContextOptions<AstrhoAppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Agendum> Agenda { get; set; }

    public virtual DbSet<Categorium> Categoria { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Compra> Compras { get; set; }

    public virtual DbSet<DetalleCompra> DetalleCompras { get; set; }

    public virtual DbSet<DetalleEntrega> DetalleEntregas { get; set; }

    public virtual DbSet<DetalleVenta> DetalleVentas { get; set; }

    public virtual DbSet<Empleado> Empleados { get; set; }

    public virtual DbSet<Entregainsumo> Entregainsumos { get; set; }

    public virtual DbSet<Estado> Estados { get; set; }

    public virtual DbSet<Horario> Horarios { get; set; }

    public virtual DbSet<HorarioDia> HorarioDias { get; set; }

    public virtual DbSet<HorarioEmpleado> HorarioEmpleados { get; set; }

    public virtual DbSet<Insumo> Insumos { get; set; }

    public virtual DbSet<Marca> Marcas { get; set; }

    public virtual DbSet<MetodoPago> MetodoPagos { get; set; }

    public virtual DbSet<Motivo> Motivos { get; set; }

    public virtual DbSet<Permiso> Permisos { get; set; }

    public virtual DbSet<Proveedor> Proveedors { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<RolPermiso> RolPermisos { get; set; }

    public virtual DbSet<Servicio> Servicios { get; set; }

    public virtual DbSet<ServicioAgendum> ServicioAgenda { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Ventum> Venta { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("workstation id=AstrhoappDB.mssql.somee.com;packet size=4096;user id=AstrhoAPP_SQLLogin_1;pwd=51b2jfaw1y;data source=AstrhoappDB.mssql.somee.com;persist security info=False;initial catalog=AstrhoappDB;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Agendum>(entity =>
        {
            entity.HasKey(e => e.AgendaId).HasName("PK__Agenda__461B3C85E20AC940");

            entity.ToTable(tb => tb.HasTrigger("TR_Agenda_VentaId_Unique"));

            entity.Property(e => e.AgendaId).HasColumnName("agenda_id");
            entity.Property(e => e.DocumentoCliente)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("documento_cliente");
            entity.Property(e => e.DocumentoEmpleado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("documento_empleado");
            entity.Property(e => e.EstadoId).HasColumnName("estado_id");
            entity.Property(e => e.FechaCita).HasColumnName("fecha_cita");
            entity.Property(e => e.HoraInicio).HasColumnName("hora_inicio");
            entity.Property(e => e.MetodopagoId).HasColumnName("metodopago_Id");
            entity.Property(e => e.Observaciones)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("observaciones");
            entity.Property(e => e.VentaId)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("venta_id");

            entity.HasOne(d => d.DocumentoClienteNavigation).WithMany(p => p.Agenda)
                .HasForeignKey(d => d.DocumentoCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Agenda_Documento_Cliente");

            entity.HasOne(d => d.DocumentoEmpleadoNavigation).WithMany(p => p.Agenda)
                .HasForeignKey(d => d.DocumentoEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Agenda_Documento_Empleado");

            entity.HasOne(d => d.Estado).WithMany(p => p.Agenda)
                .HasForeignKey(d => d.EstadoId)
                .HasConstraintName("FK_Agenda_estado_id");

            entity.HasOne(d => d.Metodopago).WithMany(p => p.Agenda)
                .HasForeignKey(d => d.MetodopagoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Agenda_metodopago_id");

            entity.HasOne(d => d.Venta).WithMany(p => p.Agenda)
                .HasForeignKey(d => d.VentaId)
                .HasConstraintName("FK_Agenda_Venta");
        });

        modelBuilder.Entity<Categorium>(entity =>
        {
            entity.HasKey(e => e.CategoriaId).HasName("PK__Categori__DB875A4FA0E0C35B");

            entity.Property(e => e.CategoriaId).HasColumnName("categoria_id");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.Estado)
                .HasDefaultValue(true)
                .HasColumnName("estado");
            entity.Property(e => e.Nombre)
                .HasMaxLength(40)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.DocumentoCliente).HasName("PK_Documento_Cliente");

            entity.ToTable("Cliente");

            entity.Property(e => e.DocumentoCliente)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("documento_cliente");
            entity.Property(e => e.Dirección)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Estado)
                .HasDefaultValue(true)
                .HasColumnName("estado");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Telefono)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("telefono");
            entity.Property(e => e.TipoDocumento)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("tipo_documento");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cliente__usuario__6C190EBB");
        });

        modelBuilder.Entity<Compra>(entity =>
        {
            entity.HasKey(e => e.CompraId).HasName("PK__Compra__7B94793C8EE98F55");

            entity.ToTable("Compra");

            entity.Property(e => e.CompraId).HasColumnName("compra_id");
            entity.Property(e => e.Estado)
                .HasDefaultValue(true)
                .HasColumnName("estado");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.Iva)
                .HasDefaultValue(19m)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("iva");
            entity.Property(e => e.ProveedorId).HasColumnName("proveedor_id");
            entity.Property(e => e.Subtotal)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("subtotal");
            entity.Property(e => e.Total)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("total");

            entity.HasOne(d => d.Proveedor).WithMany(p => p.Compras)
                .HasForeignKey(d => d.ProveedorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Compra__proveedo__6383C8BA");
        });

        modelBuilder.Entity<DetalleCompra>(entity =>
        {
            entity.HasKey(e => e.DetalleCompraId).HasName("PK__DetalleC__E25F22054AD39B08");

            entity.ToTable("DetalleCompra");

            entity.Property(e => e.DetalleCompraId).HasColumnName("detalleCompra_id");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.CompraId).HasColumnName("compra_id");
            entity.Property(e => e.InsumoId).HasColumnName("insumo_id");
            entity.Property(e => e.PrecioUnitario)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio_unitario");

            entity.HasOne(d => d.Compra).WithMany(p => p.DetalleCompras)
                .HasForeignKey(d => d.CompraId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DetalleCo__compr__66603565");

            entity.HasOne(d => d.Insumo).WithMany(p => p.DetalleCompras)
                .HasForeignKey(d => d.InsumoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleCo_Insumo");
        });

        modelBuilder.Entity<DetalleEntrega>(entity =>
        {
            entity.HasKey(e => e.DetalleEntregaId).HasName("PK__DetalleP__482B835C47786F7D");

            entity.ToTable("DetalleEntrega");

            entity.Property(e => e.DetalleEntregaId).HasColumnName("detalleEntrega_id");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.EntregainsumoId).HasColumnName("entregainsumo_id");
            entity.Property(e => e.InsumoId).HasColumnName("insumo_id");

            entity.HasOne(d => d.Entregainsumo).WithMany(p => p.DetalleEntregas)
                .HasForeignKey(d => d.EntregainsumoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DetallePr__entre__123EB7A3");

            entity.HasOne(d => d.Insumo).WithMany(p => p.DetalleEntregas)
                .HasForeignKey(d => d.InsumoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleEn_Insumo");
        });

        modelBuilder.Entity<DetalleVenta>(entity =>
        {
            entity.HasKey(e => e.DetalleVentaId).HasName("PK_DetalleVenta");

            entity.ToTable("DetalleVenta");

            entity.Property(e => e.DetalleVentaId).HasColumnName("detalleVenta_id");
            entity.Property(e => e.VentaId)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("venta_id");
            entity.Property(e => e.ServicioId).HasColumnName("servicio_id");
            entity.Property(e => e.Precio)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio");

            entity.HasOne(d => d.Venta).WithMany(p => p.DetalleVentas)
                .HasForeignKey(d => d.VentaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleVenta_Venta");

            entity.HasOne(d => d.Servicio).WithMany(p => p.DetalleVentas)
                .HasForeignKey(d => d.ServicioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DetalleVenta_Servicio");
        });

        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.HasKey(e => e.DocumentoEmpleado).HasName("PK_Documento_Empleado");

            entity.ToTable("Empleado");

            entity.Property(e => e.DocumentoEmpleado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("documento_empleado");
            entity.Property(e => e.Dirección)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Estado)
                .HasDefaultValue(true)
                .HasColumnName("estado");
            entity.Property(e => e.Nombre)
                .HasMaxLength(60)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Telefono)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("telefono");
            entity.Property(e => e.TipoDocumento)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("tipo_documento");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Empleado__usuari__7D439ABD");
        });

        modelBuilder.Entity<Entregainsumo>(entity =>
        {
            entity.HasKey(e => e.EntregainsumoId).HasName("PK__Entregai__A68F46B19994B5E2");

            entity.ToTable("Entregainsumo");

            entity.Property(e => e.EntregainsumoId).HasColumnName("entregainsumo_id");
            entity.Property(e => e.DocumentoEmpleado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("documento_empleado");
            entity.Property(e => e.EstadoId)
                .HasColumnName("estado_id");
            entity.Property(e => e.FechaCompletado)
                .HasColumnType("datetime")
                .HasColumnName("fecha_completado");
            entity.Property(e => e.FechaCreado)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_creado");
            entity.Property(e => e.FechaEntrega)
                .HasColumnType("datetime")
                .HasColumnName("fecha_entrega");
            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");

            entity.HasOne(d => d.DocumentoEmpleadoNavigation).WithMany(p => p.Entregainsumos)
                .HasForeignKey(d => d.DocumentoEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_EntregaIn_Documento_Empleado");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Entregainsumos)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Entregain__usuar__0F624AF8");

            entity.HasOne(d => d.Estado).WithMany(p => p.Entregainsumos)
                .HasForeignKey(d => d.EstadoId)
                .HasConstraintName("FK_EntregaIn_Estado");
        });

        modelBuilder.Entity<Estado>(entity =>
        {
            entity.HasKey(e => e.EstadoId).HasName("PK__Estado__053774EFAEF9DE03");

            entity.ToTable("Estado");

            entity.Property(e => e.EstadoId).HasColumnName("estado_id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Horario>(entity =>
        {
            entity.HasKey(e => e.HorarioId).HasName("PK_Horario");

            entity.ToTable("Horario");

            entity.Property(e => e.HorarioId).HasColumnName("horario_id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Estado)
                .HasDefaultValue(true)
                .HasColumnName("estado");
        });

        modelBuilder.Entity<HorarioDia>(entity =>
        {
            entity.HasKey(e => e.HorarioDiaId).HasName("PK_HorarioDia");

            entity.ToTable("HorarioDia");

            entity.Property(e => e.HorarioDiaId).HasColumnName("horarioDia_id");
            entity.Property(e => e.HorarioId).HasColumnName("horario_id");
            entity.Property(e => e.DiaSemana)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("dia_semana");
            entity.Property(e => e.HoraInicio).HasColumnName("hora_inicio");
            entity.Property(e => e.HoraFin).HasColumnName("hora_fin");

            entity.HasOne(d => d.Horario).WithMany(p => p.HorarioDias)
                .HasForeignKey(d => d.HorarioId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_HorarioDia_Horario");
        });

        modelBuilder.Entity<HorarioEmpleado>(entity =>
        {
            entity.HasKey(e => e.HorarioEmpleadoId).HasName("PK_HorarioEmpleado");

            entity.ToTable("HorarioEmpleado");

            entity.HasIndex(e => new { e.HorarioDiaId, e.DocumentoEmpleado }, "UQ_HorarioDia_Empleado").IsUnique();

            entity.Property(e => e.HorarioEmpleadoId).HasColumnName("horarioEmpleado_id");
            entity.Property(e => e.DocumentoEmpleado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("documento_empleado");
            entity.Property(e => e.HorarioDiaId).HasColumnName("horarioDia_id");

            entity.HasOne(d => d.DocumentoEmpleadoNavigation).WithMany(p => p.HorarioEmpleados)
                .HasForeignKey(d => d.DocumentoEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HorarioEmp_Documento_Empleado");

            entity.HasOne(d => d.HorarioDia).WithMany(p => p.HorarioEmpleados)
                .HasForeignKey(d => d.HorarioDiaId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_HorarioEmp_HorarioDia");
        });

        modelBuilder.Entity<Insumo>(entity =>
        {
            entity.HasKey(e => e.InsumoId).HasName("PK_Insumo_id");

            entity.ToTable("Insumo");

            entity.HasIndex(e => e.Sku, "UQ_SKU").IsUnique();

            entity.Property(e => e.InsumoId).HasColumnName("insumo_id");
            entity.Property(e => e.CategoriaId).HasColumnName("categoria_id");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.Estado)
                .HasDefaultValue(true)
                .HasColumnName("estado");
            entity.Property(e => e.Nombre)
                .HasMaxLength(80)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Sku)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("SKU");

            entity.HasOne(d => d.Categoria).WithMany(p => p.Insumos)
                .HasForeignKey(d => d.CategoriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Producto__catego__5165187F");
        });

        modelBuilder.Entity<Marca>(entity =>
        {
            entity.HasKey(e => e.MarcaId).HasName("PK__Marca__BBC43191A04C5D79");

            entity.ToTable("Marca");

            entity.Property(e => e.MarcaId).HasColumnName("marca_id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(80)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<MetodoPago>(entity =>
        {
            entity.HasKey(e => e.MetodopagoId).HasName("PK__MetodoPa__EEA4EA958277E2F2");

            entity.ToTable("MetodoPago");

            entity.Property(e => e.MetodopagoId).HasColumnName("metodopago_id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Permiso>(entity =>
        {
            entity.HasKey(e => e.PermisoId).HasName("PK__Permiso__60B569CD7DF25DF4");

            entity.ToTable("Permiso");

            entity.Property(e => e.PermisoId).HasColumnName("permiso_id");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Proveedor>(entity =>
        {
            entity.HasKey(e => e.ProveedorId).HasName("PK__Proveedo__88BBADA4F1BDA68B");

            entity.ToTable("Proveedor");

            entity.HasIndex(e => e.Documento, "UQ__Proveedo__AF73706D58875709").IsUnique();

            entity.Property(e => e.ProveedorId).HasColumnName("proveedor_id");
            entity.Property(e => e.Ciudad)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("ciudad");
            entity.Property(e => e.Correo)
                .HasMaxLength(80)
                .IsUnicode(false)
                .HasColumnName("correo");
            entity.Property(e => e.Departamento)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("departamento");
            entity.Property(e => e.Direccion)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("direccion");
            entity.Property(e => e.Documento)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.Estado)
                .HasDefaultValue(true)
                .HasColumnName("estado");
            entity.Property(e => e.Nombre)
                .HasMaxLength(80)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.PersonaContacto)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("persona_contacto");
            entity.Property(e => e.Telefono)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("telefono");
            entity.Property(e => e.TipoDocumento)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.TipoProveedor)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("tipo_proveedor");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.RolId).HasName("PK__Rol__CF32E443170986B9");

            entity.ToTable("Rol");

            entity.Property(e => e.RolId).HasColumnName("rol_id");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.Estado)
                .HasDefaultValue(true)
                .HasColumnName("estado");
            entity.Property(e => e.Nombre)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<RolPermiso>(entity =>
        {
            entity.HasKey(e => e.RolPermisoId).HasName("PK__RolPermi__8632A68FED542866");

            entity.ToTable("RolPermiso");

            entity.Property(e => e.RolPermisoId).HasColumnName("rolPermiso_id");
            entity.Property(e => e.PermisoId).HasColumnName("permiso_id");
            entity.Property(e => e.RolId).HasColumnName("rol_id");

            entity.HasOne(d => d.Permiso).WithMany(p => p.RolPermisos)
                .HasForeignKey(d => d.PermisoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RolPermis__permi__3F466844");

            entity.HasOne(d => d.Rol).WithMany(p => p.RolPermisos)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RolPermis__rol_i__3E52440B");
        });

        modelBuilder.Entity<Servicio>(entity =>
        {
            entity.HasKey(e => e.ServicioId).HasName("PK__Servicio__AF3A090C218DE604");

            entity.ToTable("Servicio");

            entity.Property(e => e.ServicioId).HasColumnName("servicio_id");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.Duracion).HasColumnName("duracion");
            entity.Property(e => e.Estado)
                .HasDefaultValue(true)
                .HasColumnName("estado");
            entity.Property(e => e.Imagen)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("imagen");
            entity.Property(e => e.Nombre)
                .HasMaxLength(60)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Precio)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio");
        });

        modelBuilder.Entity<ServicioAgendum>(entity =>
        {
            entity.HasKey(e => e.ServicioAgendaId).HasName("PK__Servicio__B789B9B3CF75B2B6");

            entity.Property(e => e.ServicioAgendaId).HasColumnName("servicioAgenda_id");
            entity.Property(e => e.AgendaId).HasColumnName("agenda_id");
            entity.Property(e => e.ServicioId).HasColumnName("servicio_id");

            entity.HasOne(d => d.Agenda).WithMany(p => p.ServicioAgenda)
                .HasForeignKey(d => d.AgendaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ServicioA__agend__09A971A2");

            entity.HasOne(d => d.Servicio).WithMany(p => p.ServicioAgenda)
                .HasForeignKey(d => d.ServicioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ServicioA__servi__08B54D69");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.UsuarioId).HasName("PK__Usuario__2ED7D2AFAC0B2A1E");

            entity.ToTable("Usuario");

            entity.Property(e => e.UsuarioId).HasColumnName("usuario_id");
            entity.Property(e => e.Contrasena)
                .HasMaxLength(64)
                .IsUnicode(false)
                .HasColumnName("contrasena");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Estado)
                .HasDefaultValue(true)
                .HasColumnName("estado");
            entity.Property(e => e.RolId).HasColumnName("rol_id");

            entity.HasOne(d => d.Rol).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Usuario__rol_id__440B1D61");
        });

        modelBuilder.Entity<Ventum>(entity =>
        {
            entity.HasKey(e => e.VentaId).HasName("PK__Venta__B1350809C97AAD0F");

            entity.Property(e => e.VentaId)
                .HasMaxLength(25)
                .IsUnicode(false)
                .HasColumnName("venta_id");
            entity.Property(e => e.DocumentoCliente)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("documento_cliente");
            entity.Property(e => e.Estado).HasColumnName("estado");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.MetodopagoId).HasColumnName("metodopago_id");
            entity.Property(e => e.Subtotal)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("subtotal");
            entity.Property(e => e.Total)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("total");

            entity.HasOne(d => d.DocumentoClienteNavigation).WithMany(p => p.Venta)
                .HasForeignKey(d => d.DocumentoCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Venta_Documento_Cliente");

            entity.HasOne(d => d.Metodopago).WithMany(p => p.Venta)
                .HasForeignKey(d => d.MetodopagoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Venta_metodopago_id");
        });

        OnModelCreatingPartial(modelBuilder);

        modelBuilder.Entity<Motivo>(entity =>
        {
            entity.HasKey(e => e.MotivoId);
            entity.Property(e => e.Descripcion).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Fecha).IsRequired().HasColumnType("date");
            entity.Property(e => e.HoraInicio).IsRequired().HasColumnType("time");
            entity.Property(e => e.HoraFin).IsRequired().HasColumnType("time");
            entity.Property(e => e.EstadoId).HasDefaultValue(1);
            entity.HasOne(e => e.Empleado).WithMany().HasForeignKey(e => e.DocumentoEmpleado);
            entity.HasOne(e => e.Estado).WithMany().HasForeignKey(e => e.EstadoId);
        });

    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
