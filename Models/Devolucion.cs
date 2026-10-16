using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistroLibro.Models;

public class Devolucion
{
    [Key]
    public int DevolucionId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un prestamo valido.")]
    public int PrestamoId { get; set; }

    [Required(ErrorMessage = "Debe indicar la fecha de devolucion.")]
    public DateTime FechaDevolucion { get; set; } = DateTime.Today;

    [ForeignKey(nameof(PrestamoId))]
    public Prestamo? Prestamo { get; set; }
}
