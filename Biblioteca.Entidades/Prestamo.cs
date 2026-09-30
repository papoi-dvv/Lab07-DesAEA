namespace Biblioteca.Entidades;

public class Prestamo
{
    public int PrestamoId { get; set; }
    public int SocioId { get; set; }
    public DateTime FechaPrestamo { get; set; }
    public DateTime FechaLimite { get; set; }
    public string Estado { get; set; } = EstadoPrestamo.Pendiente;

    // Solo para lectura (joins de reporte); no se usa en Insertar/Actualizar.
    public string? NombreSocio { get; set; }
}

public static class EstadoPrestamo
{
    public const string Pendiente = "Pendiente";
    public const string Devuelto = "Devuelto";
}
