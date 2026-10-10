using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace RegistroLibro.Models;

public class Prestamo
{
    [Key]
    public int PrestamoId { get; set; }
    [Required(ErrorMessage = "El libro no puede estar en blanco")]
    public string Libro { get; set; } = string.Empty;
    [Required(ErrorMessage = "El nombre del estudiante no puede estar en blanco")]
    public string Estudiante { get; set; } = string.Empty;
    public DateTime FechaPrestamo { get; set; } = DateTime.Today;
    [Required(ErrorMessage = "La fecha de devolucion debe ser obligatoria")]
    public DateTime FechaDevolucion { get; set; } = DateTime.Today.AddDays(7);
    public DateTime? FechaEntrega { get; set; }
    public string? Observaciones { get; set; }
}