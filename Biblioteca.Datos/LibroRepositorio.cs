using System.Data;
using Biblioteca.Entidades;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos;

public class LibroRepositorio : ILibroRepositorio
{
    public int Insertar(Libro libro)
    {
        using var conn = DbHelper.CrearConexion();
        using var cmd = new SqlCommand("dbo.sp_Libro_Insertar", conn) { CommandType = CommandType.StoredProcedure };
        AgregarParametrosComunes(cmd, libro);
        var outId = cmd.Parameters.Add("@LibroId", SqlDbType.Int);
        outId.Direction = ParameterDirection.Output;

        conn.Open();
        cmd.ExecuteNonQuery();
        libro.LibroId = (int)outId.Value;
        return libro.LibroId;
    }

    public async Task<int> InsertarAsync(Libro libro, CancellationToken ct = default)
    {
        await using var conn = DbHelper.CrearConexion();
        await using var cmd = new SqlCommand("dbo.sp_Libro_Insertar", conn) { CommandType = CommandType.StoredProcedure };
        AgregarParametrosComunes(cmd, libro);
        var outId = cmd.Parameters.Add("@LibroId", SqlDbType.Int);
        outId.Direction = ParameterDirection.Output;

        await conn.OpenAsync(ct);
        await cmd.ExecuteNonQueryAsync(ct);
        libro.LibroId = (int)outId.Value;
        return libro.LibroId;
    }

    public void Actualizar(Libro libro)
    {
        using var conn = DbHelper.CrearConexion();
        using var cmd = new SqlCommand("dbo.sp_Libro_Actualizar", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@LibroId", libro.LibroId);
        AgregarParametrosComunes(cmd, libro);

        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public async Task ActualizarAsync(Libro libro, CancellationToken ct = default)
    {
        await using var conn = DbHelper.CrearConexion();
        await using var cmd = new SqlCommand("dbo.sp_Libro_Actualizar", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@LibroId", libro.LibroId);
        AgregarParametrosComunes(cmd, libro);

        await conn.OpenAsync(ct);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    // Eliminacion logica: el SP hace UPDATE Activo = 0, nunca DELETE.
    public void EliminarLogico(int libroId)
    {
        using var conn = DbHelper.CrearConexion();
        using var cmd = new SqlCommand("dbo.sp_Libro_EliminarLogico", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@LibroId", libroId);

        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public async Task EliminarLogicoAsync(int libroId, CancellationToken ct = default)
    {
        await using var conn = DbHelper.CrearConexion();
        await using var cmd = new SqlCommand("dbo.sp_Libro_EliminarLogico", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@LibroId", libroId);

        await conn.OpenAsync(ct);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    public List<Libro> Listar()
    {
        using var conn = DbHelper.CrearConexion();
        using var cmd = new SqlCommand("dbo.sp_Libro_Listar", conn) { CommandType = CommandType.StoredProcedure };

        conn.Open();
        using var reader = cmd.ExecuteReader();
        return MapearLista(reader);
    }

    public async Task<List<Libro>> ListarAsync(CancellationToken ct = default)
    {
        await using var conn = DbHelper.CrearConexion();
        await using var cmd = new SqlCommand("dbo.sp_Libro_Listar", conn) { CommandType = CommandType.StoredProcedure };

        await conn.OpenAsync(ct);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await MapearListaAsync(reader, ct);
    }

    public Libro? BuscarPorId(int libroId)
    {
        using var conn = DbHelper.CrearConexion();
        using var cmd = new SqlCommand("dbo.sp_Libro_BuscarPorId", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@LibroId", libroId);

        conn.Open();
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapearFila(reader) : null;
    }

    public async Task<Libro?> BuscarPorIdAsync(int libroId, CancellationToken ct = default)
    {
        await using var conn = DbHelper.CrearConexion();
        await using var cmd = new SqlCommand("dbo.sp_Libro_BuscarPorId", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@LibroId", libroId);

        await conn.OpenAsync(ct);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? MapearFila(reader) : null;
    }

    public Libro? BuscarPorIsbn(string isbn)
    {
        using var conn = DbHelper.CrearConexion();
        using var cmd = new SqlCommand("dbo.sp_Libro_BuscarPorIsbn", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@ISBN", isbn);

        conn.Open();
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapearFilaSinAutor(reader) : null;
    }

    public async Task<Libro?> BuscarPorIsbnAsync(string isbn, CancellationToken ct = default)
    {
        await using var conn = DbHelper.CrearConexion();
        await using var cmd = new SqlCommand("dbo.sp_Libro_BuscarPorIsbn", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@ISBN", isbn);

        await conn.OpenAsync(ct);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? MapearFilaSinAutor(reader) : null;
    }

    public List<Libro> BuscarPorTituloOAutor(string texto)
    {
        using var conn = DbHelper.CrearConexion();
        using var cmd = new SqlCommand("dbo.sp_Libro_BuscarPorTituloOAutor", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@Texto", texto);

        conn.Open();
        using var reader = cmd.ExecuteReader();
        return MapearLista(reader);
    }

    public async Task<List<Libro>> BuscarPorTituloOAutorAsync(string texto, CancellationToken ct = default)
    {
        await using var conn = DbHelper.CrearConexion();
        await using var cmd = new SqlCommand("dbo.sp_Libro_BuscarPorTituloOAutor", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@Texto", texto);

        await conn.OpenAsync(ct);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await MapearListaAsync(reader, ct);
    }

    public bool TienePrestamosPendientes(int libroId)
    {
        using var conn = DbHelper.CrearConexion();
        using var cmd = new SqlCommand("dbo.sp_Libro_TienePrestamosPendientes", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@LibroId", libroId);
        var outFlag = cmd.Parameters.Add("@TienePendientes", SqlDbType.Bit);
        outFlag.Direction = ParameterDirection.Output;

        conn.Open();
        cmd.ExecuteNonQuery();
        return (bool)outFlag.Value;
    }

    public async Task<bool> TienePrestamosPendientesAsync(int libroId, CancellationToken ct = default)
    {
        await using var conn = DbHelper.CrearConexion();
        await using var cmd = new SqlCommand("dbo.sp_Libro_TienePrestamosPendientes", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@LibroId", libroId);
        var outFlag = cmd.Parameters.Add("@TienePendientes", SqlDbType.Bit);
        outFlag.Direction = ParameterDirection.Output;

        await conn.OpenAsync(ct);
        await cmd.ExecuteNonQueryAsync(ct);
        return (bool)outFlag.Value;
    }

    public void AjustarEjemplares(int libroId, int delta)
    {
        using var conn = DbHelper.CrearConexion();
        using var cmd = new SqlCommand("dbo.sp_Libro_AjustarEjemplares", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@LibroId", libroId);
        cmd.Parameters.AddWithValue("@Delta", delta);

        conn.Open();
        cmd.ExecuteNonQuery();
    }

    public async Task AjustarEjemplaresAsync(int libroId, int delta, CancellationToken ct = default)
    {
        await using var conn = DbHelper.CrearConexion();
        await using var cmd = new SqlCommand("dbo.sp_Libro_AjustarEjemplares", conn) { CommandType = CommandType.StoredProcedure };
        cmd.Parameters.AddWithValue("@LibroId", libroId);
        cmd.Parameters.AddWithValue("@Delta", delta);

        await conn.OpenAsync(ct);
        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static void AgregarParametrosComunes(SqlCommand cmd, Libro libro)
    {
        cmd.Parameters.AddWithValue("@Titulo", libro.Titulo);
        cmd.Parameters.AddWithValue("@ISBN", libro.ISBN);
        cmd.Parameters.AddWithValue("@AutorId", libro.AutorId);
        cmd.Parameters.AddWithValue("@Ejemplares", libro.Ejemplares);
    }

    private static List<Libro> MapearLista(SqlDataReader reader)
    {
        var lista = new List<Libro>();
        while (reader.Read())
        {
            lista.Add(MapearFila(reader));
        }
        return lista;
    }

    private static async Task<List<Libro>> MapearListaAsync(SqlDataReader reader, CancellationToken ct)
    {
        var lista = new List<Libro>();
        while (await reader.ReadAsync(ct))
        {
            lista.Add(MapearFila(reader));
        }
        return lista;
    }

    private static Libro MapearFila(SqlDataReader reader) => new()
    {
        LibroId = (int)reader["LibroId"],
        Titulo = (string)reader["Titulo"],
        ISBN = (string)reader["ISBN"],
        AutorId = (int)reader["AutorId"],
        Ejemplares = (int)reader["Ejemplares"],
        Activo = (bool)reader["Activo"],
        NombreAutor = reader["NombreAutor"] as string
    };

    private static Libro MapearFilaSinAutor(SqlDataReader reader) => new()
    {
        LibroId = (int)reader["LibroId"],
        Titulo = (string)reader["Titulo"],
        ISBN = (string)reader["ISBN"],
        AutorId = (int)reader["AutorId"],
        Ejemplares = (int)reader["Ejemplares"],
        Activo = (bool)reader["Activo"]
    };
}
