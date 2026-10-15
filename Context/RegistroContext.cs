using Microsoft.EntityFrameworkCore;
using RegistroLibro.Models;

namespace RegistroLibro.Context;

public class RegistroContext : DbContext
{
    public RegistroContext(DbContextOptions<RegistroContext> options)
        : base(options)
    {
    }

    public DbSet<Libro> Libros { get; set; }
    public DbSet<Estudiante> Estudiantes { get; set; }
    public DbSet<Prestamo> Prestamos { get; set; }
    public DbSet<Devolucion> Devoluciones { get; set; }
    public DbSet<Entrada> Entradas { get; set; }
    public DbSet<EntradasDetalle> EntradasDetalle { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Libro>().ToTable("Libro");
        modelBuilder.Entity<Libro>()
            .Property(l => l.AnoPublicacion)
            .IsRequired();
        modelBuilder.Entity<Libro>()
            .HasIndex(l => l.Titulo)
            .IsUnique();

        modelBuilder.Entity<Estudiante>().ToTable("Estudiantes");
        modelBuilder.Entity<Estudiante>()
            .Property(e => e.Nombres)
            .HasMaxLength(150)
            .UseCollation("Latin1_General_100_CI_AS")
            .IsRequired();
        modelBuilder.Entity<Estudiante>()
            .HasIndex(e => e.Nombres)
            .IsUnique();
        modelBuilder.Entity<Estudiante>()
            .Property(e => e.FechaNacimiento)
            .HasColumnType("date")
            .IsRequired();

        modelBuilder.Entity<Prestamo>().ToTable("Prestamos");

        modelBuilder.Entity<Devolucion>().ToTable("Devoluciones");
        modelBuilder.Entity<Devolucion>()
            .HasOne(d => d.Prestamo)
            .WithMany()
            .HasForeignKey(d => d.PrestamoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Entrada>().ToTable("Entradas");

        modelBuilder.Entity<Entrada>()
            .HasMany(e => e.Detalles)
            .WithOne(d => d.Entrada)
            .HasForeignKey(d => d.EntradaId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EntradasDetalle>().ToTable("EntradasDetalle");

        modelBuilder.Entity<EntradasDetalle>()
            .HasOne(d => d.Libro)
            .WithMany()
            .HasForeignKey(d => d.LibroId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
