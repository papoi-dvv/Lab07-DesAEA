using System.Data;
using Biblioteca.Entidades;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos;

public class PrestamoRepositorio
{
    public int ContarPendientesPorSocio(int socioId)
    {
        using var conn = DbHelper.CrearConexion();
        using var cmd = new SqlCommand("dbo.sp_Prestamo_ContarPendientesPorSocio", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@SocioId", socioId);
        var outCantidad = cmd.Parameters.Add("@Cantidad", SqlDbType.Int);
        outCantidad.Direction = ParameterDirection.Output;

        conn.Open();
        cmd.ExecuteNonQuery();
        return (int)outCantidad.Value;
    }

    public async Task<int> ContarPendientesPorSocioAsync(int socioId, CancellationToken ct = default)
    {
        await using var conn = DbHelper.CrearConexion();
        await using var cmd = new SqlCommand("dbo.sp_Prestamo_ContarPendientesPorSocio", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@SocioId", socioId);
        var outCantidad = cmd.Parameters.Add("@Cantidad", SqlDbType.Int);
        outCantidad.Direction = ParameterDirection.Output;

        await conn.OpenAsync(ct);
        await cmd.ExecuteNonQueryAsync(ct);
        return (int)outCantidad.Value;
    }

    public Prestamo? BuscarPorId(int prestamoId)
    {
        using var conn = DbHelper.CrearConexion();
        using var cmd = new SqlCommand("dbo.sp_Prestamo_BuscarPorId", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@PrestamoId", prestamoId);

        conn.Open();
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapearPrestamo(reader) : null;
    }

    public async Task<Prestamo?> BuscarPorIdAsync(int prestamoId, CancellationToken ct = default)
    {
        await using var conn = DbHelper.CrearConexion();
        await using var cmd = new SqlCommand("dbo.sp_Prestamo_BuscarPorId", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@PrestamoId", prestamoId);

        await conn.OpenAsync(ct);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? MapearPrestamo(reader) : null;
    }

    public List<DetallePrestamo> ListarDetallePorPrestamo(int prestamoId)
    {
        using var conn = DbHelper.CrearConexion();
        using var cmd = new SqlCommand("dbo.sp_DetallePrestamo_ListarPorPrestamo", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@PrestamoId", prestamoId);

        conn.Open();
        using var reader = cmd.ExecuteReader();
        return MapearDetalleLista(reader);
    }

    public async Task<List<DetallePrestamo>> ListarDetallePorPrestamoAsync(int prestamoId, CancellationToken ct = default)
    {
        await using var conn = DbHelper.CrearConexion();
        await using var cmd = new SqlCommand("dbo.sp_DetallePrestamo_ListarPorPrestamo", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@PrestamoId", prestamoId);

        await conn.OpenAsync(ct);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await MapearDetalleListaAsync(reader, ct);
    }

    /// <summary>
    /// Inserta la cabecera del prestamo, un detalle por cada libro y descuenta el stock
    /// de cada uno, todo dentro de una unica transaccion: si algo falla no se guarda nada.
    /// Las reglas de negocio (limite de 3 pendientes, stock, Activo) ya fueron validadas
    /// por PrestamoNegocio antes de llamar a este metodo.
    /// </summary>
    public async Task<int> RegistrarPrestamoAsync(int socioId, DateTime fechaPrestamo, DateTime fechaLimite,
        IReadOnlyCollection<int> libroIds, CancellationToken ct = default)
    {
        await using var conn = DbHelper.CrearConexion();
        await conn.OpenAsync(ct);
        await using var tx = (SqlTransaction)await conn.BeginTransactionAsync(ct);
        try
        {
            int prestamoId;
            await using (var cmdPrestamo = new SqlCommand("dbo.sp_Prestamo_Insertar", conn, tx) { CommandType = CommandType.StoredProcedure })
            {
                cmdPrestamo.Parameters.AddWithValue("@SocioId", socioId);
                cmdPrestamo.Parameters.AddWithValue("@FechaPrestamo", fechaPrestamo);
                cmdPrestamo.Parameters.AddWithValue("@FechaLimite", fechaLimite);
                var outId = cmdPrestamo.Parameters.Add("@PrestamoId", SqlDbType.Int);
                outId.Direction = ParameterDirection.Output;

                await cmdPrestamo.ExecuteNonQueryAsync(ct);
                prestamoId = (int)outId.Value;
            }

            foreach (var libroId in libroIds)
            {
                await using var cmdDetalle = new SqlCommand("dbo.sp_DetallePrestamo_Insertar", conn, tx) { CommandType = CommandType.StoredProcedure };
                cmdDetalle.Parameters.AddWithValue("@PrestamoId", prestamoId);
                cmdDetalle.Parameters.AddWithValue("@LibroId", libroId);
                await cmdDetalle.ExecuteNonQueryAsync(ct);

                await using var cmdStock = new SqlCommand("dbo.sp_Libro_AjustarEjemplares", conn, tx) { CommandType = CommandType.StoredProcedure };
                cmdStock.Parameters.AddWithValue("@LibroId", libroId);
                cmdStock.Parameters.AddWithValue("@Delta", -1);
                await cmdStock.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);
            return prestamoId;
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    /// <summary>
    /// Registra la devolucion de un libro, repone su ejemplar y aplica el estado indicado
    /// por la capa de Negocio. Todo se confirma dentro de una unica transaccion.
    /// </summary>
    public async Task RegistrarDevolucionAsync(int prestamoId, int libroId, DateTime fechaDevolucion,
        bool marcarPrestamoDevuelto, CancellationToken ct = default)
    {
        await using var conn = DbHelper.CrearConexion();
        await conn.OpenAsync(ct);
        await using var tx = (SqlTransaction)await conn.BeginTransactionAsync(ct);
        try
        {
            await using (var cmdDevolucion = new SqlCommand("dbo.sp_Prestamo_RegistrarDevolucion", conn, tx) { CommandType = CommandType.StoredProcedure })
            {
                cmdDevolucion.Parameters.AddWithValue("@PrestamoId", prestamoId);
                cmdDevolucion.Parameters.AddWithValue("@LibroId", libroId);
                cmdDevolucion.Parameters.AddWithValue("@FechaDevolucion", fechaDevolucion);
                await cmdDevolucion.ExecuteNonQueryAsync(ct);
            }

            await using (var cmdStock = new SqlCommand("dbo.sp_Libro_AjustarEjemplares", conn, tx) { CommandType = CommandType.StoredProcedure })
            {
                cmdStock.Parameters.AddWithValue("@LibroId", libroId);
                cmdStock.Parameters.AddWithValue("@Delta", 1);
                await cmdStock.ExecuteNonQueryAsync(ct);
            }

            if (marcarPrestamoDevuelto)
            {
                await using var cmdEstado = new SqlCommand("dbo.sp_Prestamo_ActualizarEstado", conn, tx) { CommandType = CommandType.StoredProcedure };
                cmdEstado.Parameters.AddWithValue("@PrestamoId", prestamoId);
                cmdEstado.Parameters.AddWithValue("@Estado", EstadoPrestamo.Devuelto);
                await cmdEstado.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    public List<ReportePrestamoItem> ReportePorRangoFechas(DateTime fechaInicio, DateTime fechaFin)
    {
        using var conn = DbHelper.CrearConexion();
        using var cmd = new SqlCommand("dbo.sp_Prestamo_ReportePorRangoFechas", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio.Date);
        cmd.Parameters.AddWithValue("@FechaFin", fechaFin.Date);

        conn.Open();
        using var reader = cmd.ExecuteReader();
        return MapearReporteLista(reader);
    }

    public async Task<List<ReportePrestamoItem>> ReportePorRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin, CancellationToken ct = default)
    {
        await using var conn = DbHelper.CrearConexion();
        await using var cmd = new SqlCommand("dbo.sp_Prestamo_ReportePorRangoFechas", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio.Date);
        cmd.Parameters.AddWithValue("@FechaFin", fechaFin.Date);

        await conn.OpenAsync(ct);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await MapearReporteListaAsync(reader, ct);
    }

    private static Prestamo MapearPrestamo(SqlDataReader reader) => new()
    {
        PrestamoId = (int)reader["PrestamoId"],
        SocioId = (int)reader["SocioId"],
        FechaPrestamo = (DateTime)reader["FechaPrestamo"],
        FechaLimite = (DateTime)reader["FechaLimite"],
        Estado = (string)reader["Estado"]
    };

    private static List<DetallePrestamo> MapearDetalleLista(SqlDataReader reader)
    {
        var lista = new List<DetallePrestamo>();
        while (reader.Read())
        {
            lista.Add(MapearDetalle(reader));
        }
        return lista;
    }

    private static async Task<List<DetallePrestamo>> MapearDetalleListaAsync(SqlDataReader reader, CancellationToken ct)
    {
        var lista = new List<DetallePrestamo>();
        while (await reader.ReadAsync(ct))
        {
            lista.Add(MapearDetalle(reader));
        }
        return lista;
    }

    private static DetallePrestamo MapearDetalle(SqlDataReader reader) => new()
    {
        PrestamoId = (int)reader["PrestamoId"],
        LibroId = (int)reader["LibroId"],
        FechaDevolucion = reader["FechaDevolucion"] as DateTime?,
        TituloLibro = reader["TituloLibro"] as string
    };

    private static List<ReportePrestamoItem> MapearReporteLista(SqlDataReader reader)
    {
        var lista = new List<ReportePrestamoItem>();
        while (reader.Read())
        {
            lista.Add(MapearReporteItem(reader));
        }
        return lista;
    }

    private static async Task<List<ReportePrestamoItem>> MapearReporteListaAsync(SqlDataReader reader, CancellationToken ct)
    {
        var lista = new List<ReportePrestamoItem>();
        while (await reader.ReadAsync(ct))
        {
            lista.Add(MapearReporteItem(reader));
        }
        return lista;
    }

    private static ReportePrestamoItem MapearReporteItem(SqlDataReader reader) => new()
    {
        PrestamoId = (int)reader["PrestamoId"],
        NombreSocio = (string)reader["NombreSocio"],
        TituloLibro = (string)reader["TituloLibro"],
        FechaLimite = (DateTime)reader["FechaLimite"],
        Estado = (string)reader["Estado"]
    };
}
