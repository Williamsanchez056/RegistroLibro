using System.ComponentModel.DataAnnotations;

namespace RegistroLibro.Models;

public class Entrada
{
    [Key]
    public int EntradaId { get; set; }

    public DateTime Fecha { get; set; } = DateTime.Today;

    public int Total { get; set; }

    public List<EntradasDetalle> Detalles { get; set; } = new();
}
