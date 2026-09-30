using Biblioteca.Datos;
using Biblioteca.Entidades;

namespace Biblioteca.Negocio;

public class PrestamoNegocio
{
    private const int MaximoLibrosPendientesPorSocio = 3;
    private const decimal MultaPorDiaDeRetraso = 1.50m;

    private readonly PrestamoRepositorio _prestamoRepositorio;
    private readonly ILibroRepositorio _libroRepositorio;
    private readonly SocioRepositorio _socioRepositorio;

    public PrestamoNegocio(PrestamoRepositorio prestamoRepositorio, ILibroRepositorio libroRepositorio, SocioRepositorio socioRepositorio)
    {
        _prestamoRepositorio = prestamoRepositorio;
        _libroRepositorio = libroRepositorio;
        _socioRepositorio = socioRepositorio;
    }

    /// <summary>
    /// Registra un prestamo con uno o varios libros. Valida las reglas de negocio antes de
    /// delegar a Datos, que guarda cabecera, detalle y descuento de ejemplares en una sola
    /// transaccion.
    /// </summary>
    public async Task<int> RegistrarPrestamoAsync(int socioId, IReadOnlyCollection<int> libroIds,
        DateTime fechaPrestamo, DateTime fechaLimite, CancellationToken ct = default)
    {
        if (libroIds.Count == 0)
        {
            throw new ReglaNegocioException("Debe seleccionar al menos un libro para el prestamo.");
        }

        var socio = await _socioRepositorio.BuscarPorIdAsync(socioId, ct)
            ?? throw new ReglaNegocioException("El socio indicado no existe.");
        if (!socio.Activo)
        {
            throw new ReglaNegocioException("No se puede prestar un libro a un socio dado de baja.");
        }

        var pendientesActuales = await _prestamoRepositorio.ContarPendientesPorSocioAsync(socioId, ct);
        if (pendientesActuales + libroIds.Count > MaximoLibrosPendientesPorSocio)
        {
            throw new ReglaNegocioException(
                $"El socio no puede tener mas de {MaximoLibrosPendientesPorSocio} libros pendientes " +
                $"(tiene {pendientesActuales} y esta solicitando {libroIds.Count}).");
        }

        foreach (var libroId in libroIds)
        {
            var libro = await _libroRepositorio.BuscarPorIdAsync(libroId, ct)
                ?? throw new ReglaNegocioException($"El libro con id {libroId} no existe.");
            if (!libro.Activo)
            {
                throw new ReglaNegocioException($"No se puede prestar el libro '{libro.Titulo}' porque esta dado de baja.");
            }
            if (libro.Ejemplares <= 0)
            {
                throw new ReglaNegocioException($"No quedan ejemplares disponibles del libro '{libro.Titulo}'.");
            }
        }

        return await _prestamoRepositorio.RegistrarPrestamoAsync(socioId, fechaPrestamo, fechaLimite, libroIds, ct);
    }

    /// <summary>
    /// Registra la devolucion de un libro de un prestamo y calcula la multa por retraso.
    /// </summary>
    public async Task<decimal> RegistrarDevolucionAsync(int prestamoId, int libroId, DateTime fechaDevolucion, CancellationToken ct = default)
    {
        var prestamo = await _prestamoRepositorio.BuscarPorIdAsync(prestamoId, ct)
            ?? throw new ReglaNegocioException("El prestamo indicado no existe.");

        var detalle = (await _prestamoRepositorio.ListarDetallePorPrestamoAsync(prestamoId, ct))
            .FirstOrDefault(d => d.LibroId == libroId)
            ?? throw new ReglaNegocioException("El libro indicado no pertenece a este prestamo.");

        if (detalle.FechaDevolucion is not null)
        {
            throw new ReglaNegocioException("Ese libro del prestamo ya fue devuelto.");
        }

        await _prestamoRepositorio.RegistrarDevolucionAsync(prestamoId, libroId, fechaDevolucion, ct);

        return CalcularMulta(prestamo.FechaLimite, fechaDevolucion);
    }

    public static decimal CalcularMulta(DateTime fechaLimite, DateTime fechaDevolucion)
    {
        var diasDeRetraso = (fechaDevolucion.Date - fechaLimite.Date).Days;
        return diasDeRetraso > 0 ? diasDeRetraso * MultaPorDiaDeRetraso : 0m;
    }

    public Task<List<ReportePrestamoItem>> ReportePorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken ct = default) =>
        _prestamoRepositorio.ReportePorRangoFechasAsync(fechaInicio, fechaFin, ct);
}
