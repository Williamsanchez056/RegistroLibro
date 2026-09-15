using System.ComponentModel.DataAnnotations;

namespace RegistroLibro.Models;

public class Libro
{
    [Key]
    public int LibroId { get; set; }

    [Required(ErrorMessage = "Debes de poner el titulo")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Autor es obligatorio.")]
    public string Autor { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debes de introducir el año de publicación.")]
    [Range(1999, 2026, ErrorMessage = "Escribe un año entre 1 y 2026.")]
    public int? AnoPublicacion { get; set; }
}