using System.ComponentModel.DataAnnotations;
namespace RegistroLibro.Models;

public class Prestamo
{
    [Key]
    public int PrestamoId { get; set; }

    [Required(ErrorMessage = "El nombre del libro no puede estar en blanco")]
    public string Libro { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre del estudiante no puede estar en blanco")]
    public string Estudiante { get; set; } = string.Empty;

    public DateTime FechaPrestamo { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "La fecha de devolucion es obligatoria")]
    public DateTime FechaDevolucion { get; set; } = DateTime.Today.AddDays(7);
}