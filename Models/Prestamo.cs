using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RegistroLibro.Models;

public class Prestamo
{
    [Key]
    public int PrestamoId { get; set; }

    [Required(ErrorMessage = "Selecciona un libro")]
    public int LibroId { get; set; }

    [ForeignKey(nameof(LibroId))]
    public Libro? Libro { get; set; }

    [Required(ErrorMessage = "El nombre del estudiante no puede estar en blanco")]
    public string Estudiante { get; set; } = string.Empty;

    public DateTime FechaPrestamo { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "La fecha de debolucion debe ser obligatoria")]
    public DateTime FechaDevolucion { get; set; } = DateTime.Today.AddDays(7);

    public string? Observaciones { get; set; }
}