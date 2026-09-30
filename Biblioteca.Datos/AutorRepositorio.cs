using System.Data;
using Biblioteca.Entidades;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos;

public class AutorRepositorio
{
    public int Insertar(Autor autor)
    {
        using var conn = DbHelper.CrearConexion();
        using var cmd = new SqlCommand("dbo.sp_Autor_Insertar", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@Nombre", autor.Nombre);
        cmd.Parameters.AddWithValue("@Nacionalidad", autor.Nacionalidad);
        var outId = cmd.Parameters.Add("@AutorId", SqlDbType.Int);
        outId.Direction = ParameterDirection.Output;

        conn.Open();
        cmd.ExecuteNonQuery();
        autor.AutorId = (int)outId.Value;
        return autor.AutorId;
    }

    public async Task<int> InsertarAsync(Autor autor, CancellationToken ct = default)
    {
        await using var conn = DbHelper.CrearConexion();
        await using var cmd = new SqlCommand("dbo.sp_Autor_Insertar", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@Nombre", autor.Nombre);
        cmd.Parameters.AddWithValue("@Nacionalidad", autor.Nacionalidad);
        var outId = cmd.Parameters.Add("@AutorId", SqlDbType.Int);
        outId.Direction = ParameterDirection.Output;

        await conn.OpenAsync(ct);
        await cmd.ExecuteNonQueryAsync(ct);
        autor.AutorId = (int)outId.Value;
        return autor.AutorId;
    }

    public void Actualizar(Autor autor)
    {
        using var conn = DbHelper.CrearConexion();
        using var cmd = new SqlCommand("dbo.sp_Autor_Actualizar", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@AutorId", autor.AutorId);
        cmd.Parameters.AddWithValue("@Nombre", autor.Nombre);
        cmd.Parameters.AddWithValue("@Nacionalidad", autor.Nacionalidad);

        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public async Task ActualizarAsync(Autor autor, CancellationToken ct = default)
    {
        await using var conn = DbHelper.CrearConexion();
        await using var cmd = new SqlCommand("dbo.sp_Autor_Actualizar", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@AutorId", autor.AutorId);
        cmd.Parameters.AddWithValue("@Nombre", autor.Nombre);
        cmd.Parameters.AddWithValue("@Nacionalidad", autor.Nacionalidad);

        await conn.OpenAsync(ct);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    // Eliminacion logica: el SP hace UPDATE Activo = 0, nunca DELETE.
    public void EliminarLogico(int autorId)
    {
        using var conn = DbHelper.CrearConexion();
        using var cmd = new SqlCommand("dbo.sp_Autor_EliminarLogico", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@AutorId", autorId);

        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public async Task EliminarLogicoAsync(int autorId, CancellationToken ct = default)
    {
        await using var conn = DbHelper.CrearConexion();
        await using var cmd = new SqlCommand("dbo.sp_Autor_EliminarLogico", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@AutorId", autorId);

        await conn.OpenAsync(ct);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public List<Autor> Listar()
    {
        using var conn = DbHelper.CrearConexion();
        using var cmd = new SqlCommand("dbo.sp_Autor_Listar", conn) { CommandType = CommandType.StoredProcedure };

        conn.Open();
        using var reader = cmd.ExecuteReader();
        return MapearLista(reader);
    }

    public async Task<List<Autor>> ListarAsync(CancellationToken ct = default)
    {
        await using var conn = DbHelper.CrearConexion();
        await using var cmd = new SqlCommand("dbo.sp_Autor_Listar", conn) { CommandType = CommandType.StoredProcedure };

        await conn.OpenAsync(ct);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await MapearListaAsync(reader, ct);
    }

    public Autor? BuscarPorId(int autorId)
    {
        using var conn = DbHelper.CrearConexion();
        using var cmd = new SqlCommand("dbo.sp_Autor_BuscarPorId", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@AutorId", autorId);

        conn.Open();
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapearFila(reader) : null;
    }

    public async Task<Autor?> BuscarPorIdAsync(int autorId, CancellationToken ct = default)
    {
        await using var conn = DbHelper.CrearConexion();
        await using var cmd = new SqlCommand("dbo.sp_Autor_BuscarPorId", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@AutorId", autorId);

        await conn.OpenAsync(ct);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? MapearFila(reader) : null;
    }

    private static List<Autor> MapearLista(SqlDataReader reader)
    {
        var lista = new List<Autor>();
        while (reader.Read())
        {
            lista.Add(MapearFila(reader));
        }
        return lista;
    }

    private static async Task<List<Autor>> MapearListaAsync(SqlDataReader reader, CancellationToken ct)
    {
        var lista = new List<Autor>();
        while (await reader.ReadAsync(ct))
        {
            lista.Add(MapearFila(reader));
        }
        return lista;
    }

    private static Autor MapearFila(SqlDataReader reader) => new()
    {
        AutorId = (int)reader["AutorId"],
        Nombre = (string)reader["Nombre"],
        Nacionalidad = (string)reader["Nacionalidad"],
        Activo = (bool)reader["Activo"]
    };
}
