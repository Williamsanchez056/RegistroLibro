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
    }
}