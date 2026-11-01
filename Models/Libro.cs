using System.ComponentModel.DataAnnotations;

namespace RegistroLibro.Models;

public class Libro
{
    [Key]
    public int LibroId { get; set; }

    [Required(ErrorMessage = "Debes introducir el titulo.")]
    [StringLength(200, ErrorMessage = "El titulo no puede superar los 200 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El autor es obligatorio.")]
    [StringLength(150, ErrorMessage = "El autor no puede superar los 150 caracteres.")]
    public string Autor { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debes introducir el ano de publicacion.")]
    [Range(1000, 2100, ErrorMessage = "Escribe un ano valido entre 1000 y 2100.")]
    public int? AnoPublicacion { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "La cantidad disponible no puede ser negativa.")]
    public int CantidadDisponible { get; set; } = 0;
}