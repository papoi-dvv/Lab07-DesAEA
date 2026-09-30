using System.Data;
using Biblioteca.Entidades;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos;

public class SocioRepositorio
{
    public int Insertar(Socio socio)
    {
        using var conn = DbHelper.CrearConexion();
        using var cmd = new SqlCommand("dbo.sp_Socio_Insertar", conn) { CommandType = CommandType.StoredProcedure };
        AgregarParametrosComunes(cmd, socio);
        var outId = cmd.Parameters.Add("@SocioId", SqlDbType.Int);
        outId.Direction = ParameterDirection.Output;

        conn.Open();
        cmd.ExecuteNonQuery();
        socio.SocioId = (int)outId.Value;
        return socio.SocioId;
    }

    public async Task<int> InsertarAsync(Socio socio, CancellationToken ct = default)
    {
        await using var conn = DbHelper.CrearConexion();
        await using var cmd = new SqlCommand("dbo.sp_Socio_Insertar", conn) { CommandType = CommandType.StoredProcedure };
        AgregarParametrosComunes(cmd, socio);
        var outId = cmd.Parameters.Add("@SocioId", SqlDbType.Int);
        outId.Direction = ParameterDirection.Output;

        await conn.OpenAsync(ct);
        await cmd.ExecuteNonQueryAsync(ct);
        socio.SocioId = (int)outId.Value;
        return socio.SocioId;
    }

    public void Actualizar(Socio socio)
    {
        using var conn = DbHelper.CrearConexion();
        using var cmd = new SqlCommand("dbo.sp_Socio_Actualizar", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@SocioId", socio.SocioId);
        AgregarParametrosComunes(cmd, socio);

        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public async Task ActualizarAsync(Socio socio, CancellationToken ct = default)
    {
        await using var conn = DbHelper.CrearConexion();
        await using var cmd = new SqlCommand("dbo.sp_Socio_Actualizar", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@SocioId", socio.SocioId);
        AgregarParametrosComunes(cmd, socio);

        await conn.OpenAsync(ct);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    // Eliminacion logica: el SP hace UPDATE Activo = 0, nunca DELETE.
    public void EliminarLogico(int socioId)
    {
        using var conn = DbHelper.CrearConexion();
        using var cmd = new SqlCommand("dbo.sp_Socio_EliminarLogico", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@SocioId", socioId);

        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public async Task EliminarLogicoAsync(int socioId, CancellationToken ct = default)
    {
        await using var conn = DbHelper.CrearConexion();
        await using var cmd = new SqlCommand("dbo.sp_Socio_EliminarLogico", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@SocioId", socioId);

        await conn.OpenAsync(ct);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public List<Socio> Listar()
    {
        using var conn = DbHelper.CrearConexion();
        using var cmd = new SqlCommand("dbo.sp_Socio_Listar", conn) { CommandType = CommandType.StoredProcedure };

        conn.Open();
        using var reader = cmd.ExecuteReader();
        return MapearLista(reader);
    }

    public async Task<List<Socio>> ListarAsync(CancellationToken ct = default)
    {
        await using var conn = DbHelper.CrearConexion();
        await using var cmd = new SqlCommand("dbo.sp_Socio_Listar", conn) { CommandType = CommandType.StoredProcedure };

        await conn.OpenAsync(ct);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await MapearListaAsync(reader, ct);
    }

    public Socio? BuscarPorId(int socioId)
    {
        using var conn = DbHelper.CrearConexion();
        using var cmd = new SqlCommand("dbo.sp_Socio_BuscarPorId", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@SocioId", socioId);

        conn.Open();
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapearFila(reader) : null;
    }

    public async Task<Socio?> BuscarPorIdAsync(int socioId, CancellationToken ct = default)
    {
        await using var conn = DbHelper.CrearConexion();
        await using var cmd = new SqlCommand("dbo.sp_Socio_BuscarPorId", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@SocioId", socioId);

        await conn.OpenAsync(ct);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? MapearFila(reader) : null;
    }

    public Socio? BuscarPorDni(string dni)
    {
        using var conn = DbHelper.CrearConexion();
        using var cmd = new SqlCommand("dbo.sp_Socio_BuscarPorDni", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@DNI", dni);

        conn.Open();
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapearFila(reader) : null;
    }

    public async Task<Socio?> BuscarPorDniAsync(string dni, CancellationToken ct = default)
    {
        await using var conn = DbHelper.CrearConexion();
        await using var cmd = new SqlCommand("dbo.sp_Socio_BuscarPorDni", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@DNI", dni);

        await conn.OpenAsync(ct);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? MapearFila(reader) : null;
    }

    public List<Socio> BuscarPorNombreODni(string texto)
    {
        using var conn = DbHelper.CrearConexion();
        using var cmd = new SqlCommand("dbo.sp_Socio_BuscarPorNombreODni", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@Texto", texto);

        conn.Open();
        using var reader = cmd.ExecuteReader();
        return MapearLista(reader);
    }

    public async Task<List<Socio>> BuscarPorNombreODniAsync(string texto, CancellationToken ct = default)
    {
        await using var conn = DbHelper.CrearConexion();
        await using var cmd = new SqlCommand("dbo.sp_Socio_BuscarPorNombreODni", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@Texto", texto);

        await conn.OpenAsync(ct);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await MapearListaAsync(reader, ct);
    }

    public bool TienePrestamosPendientes(int socioId)
    {
        using var conn = DbHelper.CrearConexion();
        using var cmd = new SqlCommand("dbo.sp_Socio_TienePrestamosPendientes", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@SocioId", socioId);
        var outFlag = cmd.Parameters.Add("@TienePendientes", SqlDbType.Bit);
        outFlag.Direction = ParameterDirection.Output;

        conn.Open();
        cmd.ExecuteNonQuery();
        return (bool)outFlag.Value;
    }

    public async Task<bool> TienePrestamosPendientesAsync(int socioId, CancellationToken ct = default)
    {
        await using var conn = DbHelper.CrearConexion();
        await using var cmd = new SqlCommand("dbo.sp_Socio_TienePrestamosPendientes", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@SocioId", socioId);
        var outFlag = cmd.Parameters.Add("@TienePendientes", SqlDbType.Bit);
        outFlag.Direction = ParameterDirection.Output;

        await conn.OpenAsync(ct);
        await cmd.ExecuteNonQueryAsync(ct);
        return (bool)outFlag.Value;
    }

    private static void AgregarParametrosComunes(SqlCommand cmd, Socio socio)
    {
        cmd.Parameters.AddWithValue("@DNI", socio.DNI);
        cmd.Parameters.AddWithValue("@Nombre", socio.Nombre);
        cmd.Parameters.AddWithValue("@Email", socio.Email);
    }

    private static List<Socio> MapearLista(SqlDataReader reader)
    {
        var lista = new List<Socio>();
        while (reader.Read())
        {
            lista.Add(MapearFila(reader));
        }
        return lista;
    }

    private static async Task<List<Socio>> MapearListaAsync(SqlDataReader reader, CancellationToken ct)
    {
        var lista = new List<Socio>();
        while (await reader.ReadAsync(ct))
        {
            lista.Add(MapearFila(reader));
        }
        return lista;
    }

    private static Socio MapearFila(SqlDataReader reader) => new()
    {
        SocioId = (int)reader["SocioId"],
        DNI = (string)reader["DNI"],
        Nombre = (string)reader["Nombre"],
        Email = (string)reader["Email"],
        Activo = (bool)reader["Activo"]
    };
}
