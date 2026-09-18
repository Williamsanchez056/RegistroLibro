using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RegistroLibro.Models;

[Table("Estudiantes")]
public class Estudiante
{
    [Key]
    public int EstudianteId { get; set; }

    [Required(ErrorMessage = "Debes introducir el nombre.")]
    [StringLength(150)]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debes introducir la direccion.")]
    [StringLength(250)]
    public string Direccion { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debes introducir el email.")]
    [EmailAddress(ErrorMessage = "Introduce un email valido.")]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debes introducir la fecha de nacimiento.")]
    [DataType(DataType.Date)]
    public DateTime? FechaNacimiento { get; set; }
}