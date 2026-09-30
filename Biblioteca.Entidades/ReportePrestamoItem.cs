namespace Biblioteca.Entidades;

// Proyeccion de solo lectura para el reporte de prestamos por rango de fechas (req. #14):
// una fila por cada libro del prestamo (resultado del INNER JOIN de las 4 tablas).
public class ReportePrestamoItem
{
    public int PrestamoId { get; set; }
    public string NombreSocio { get; set; } = string.Empty;
    public string TituloLibro { get; set; } = string.Empty;
    public DateTime FechaLimite { get; set; }
    public string Estado { get; set; } = string.Empty;
}
