using System.Data;
using Biblioteca.Entidades;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos;

/// <summary>
/// Acceso a datos del detalle de un prestamo. Las escrituras que forman parte del alta o
/// devolucion se coordinan desde PrestamoRepositorio para conservar una sola transaccion.
/// </summary>
public class DetallePrestamoRepositorio
{
    public async Task<List<DetallePrestamo>> ListarPorPrestamoAsync(int prestamoId,
        CancellationToken ct = default)
    {
        await using var conn = DbHelper.CrearConexion();
        await using var cmd = new SqlCommand("dbo.sp_DetallePrestamo_ListarPorPrestamo", conn)
        {
            CommandType = CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@PrestamoId", prestamoId);

        await conn.OpenAsync(ct);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        var detalles = new List<DetallePrestamo>();
        while (await reader.ReadAsync(ct))
        {
            detalles.Add(new DetallePrestamo
            {
                PrestamoId = (int)reader["PrestamoId"],
                LibroId = (int)reader["LibroId"],
                FechaDevolucion = reader["FechaDevolucion"] as DateTime?,
                TituloLibro = reader["TituloLibro"] as string
            });
        }

        return detalles;
    }
}
