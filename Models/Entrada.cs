using System.ComponentModel.DataAnnotations;

namespace RegistroLibro.Models;

public class Entrada
{
    [Key]
    public int EntradaId { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "El concepto es obligatorio.")]
    public string Concepto { get; set; } = string.Empty;

    public int Total { get; set; }

    public List<EntradasDetalle> Detalles { get; set; } = new();
}
