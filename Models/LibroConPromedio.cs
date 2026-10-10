namespace RegistroLibro.Models;

public class LibroConPromedio
{
    public int LibroId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Autor { get; set; } = string.Empty;
    public double PromedioDias { get; set; }
}
