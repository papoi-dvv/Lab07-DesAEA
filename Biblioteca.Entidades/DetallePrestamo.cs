namespace Biblioteca.Entidades;

public class DetallePrestamo
{
    public int PrestamoId { get; set; }
    public int LibroId { get; set; }
    public DateTime? FechaDevolucion { get; set; }

    // Solo para lectura (joins de reporte); no se usa en Insertar/Actualizar.
    public string? TituloLibro { get; set; }
}
