using Biblioteca.Datos;
using Biblioteca.Entidades;

namespace Biblioteca.Negocio;

public class LibroNegocio
{
    private readonly ILibroRepositorio _libroRepositorio;
    private readonly AutorRepositorio _autorRepositorio;

    // Punto extra (#17): se recibe la interfaz por constructor en lugar de la clase concreta.
    public LibroNegocio(ILibroRepositorio libroRepositorio, AutorRepositorio autorRepositorio)
    {
        _libroRepositorio = libroRepositorio;
        _autorRepositorio = autorRepositorio;
    }

    public List<Libro> Listar() => _libroRepositorio.Listar();

    public Task<List<Libro>> ListarAsync(CancellationToken ct = default) => _libroRepositorio.ListarAsync(ct);

    public Task<List<Autor>> ListarAutoresAsync(CancellationToken ct = default) =>
        _autorRepositorio.ListarAsync(ct);

    public List<Libro> BuscarPorTituloOAutor(string texto) => _libroRepositorio.BuscarPorTituloOAutor(texto);

    public Task<List<Libro>> BuscarPorTituloOAutorAsync(string texto, CancellationToken ct = default) =>
        _libroRepositorio.BuscarPorTituloOAutorAsync(texto, ct);

    public int Insertar(Libro libro)
    {
        ValidarIsbnDuplicado(libro);
        return _libroRepositorio.Insertar(libro);
    }

    public async Task<int> InsertarAsync(Libro libro, CancellationToken ct = default)
    {
        ValidarCampos(libro);
        await ValidarIsbnDuplicadoAsync(libro, ct);
        return await _libroRepositorio.InsertarAsync(libro, ct);
    }

    public void Actualizar(Libro libro)
    {
        ValidarIsbnDuplicado(libro);
        _libroRepositorio.Actualizar(libro);
    }

    public async Task ActualizarAsync(Libro libro, CancellationToken ct = default)
    {
        ValidarCampos(libro);
        await ValidarIsbnDuplicadoAsync(libro, ct);
        await _libroRepositorio.ActualizarAsync(libro, ct);
    }

    public void EliminarLogico(int libroId)
    {
        if (_libroRepositorio.TienePrestamosPendientes(libroId))
        {
            throw new ReglaNegocioException("No se puede dar de baja un libro con prestamos pendientes.");
        }
        _libroRepositorio.EliminarLogico(libroId);
    }

    public async Task EliminarLogicoAsync(int libroId, CancellationToken ct = default)
    {
        if (await _libroRepositorio.TienePrestamosPendientesAsync(libroId, ct))
        {
            throw new ReglaNegocioException("No se puede dar de baja un libro con prestamos pendientes.");
        }
        await _libroRepositorio.EliminarLogicoAsync(libroId, ct);
    }

    private void ValidarIsbnDuplicado(Libro libro)
    {
        var existente = _libroRepositorio.BuscarPorIsbn(libro.ISBN);
        if (existente is not null && existente.LibroId != libro.LibroId)
        {
            throw new ReglaNegocioException($"Ya existe un libro registrado con el ISBN '{libro.ISBN}'.");
        }
    }

    private async Task ValidarIsbnDuplicadoAsync(Libro libro, CancellationToken ct)
    {
        var existente = await _libroRepositorio.BuscarPorIsbnAsync(libro.ISBN, ct);
        if (existente is not null && existente.LibroId != libro.LibroId)
        {
            throw new ReglaNegocioException($"Ya existe un libro registrado con el ISBN '{libro.ISBN}'.");
        }
    }

    private static void ValidarCampos(Libro libro)
    {
        if (string.IsNullOrWhiteSpace(libro.Titulo) || string.IsNullOrWhiteSpace(libro.ISBN))
        {
            throw new ReglaNegocioException("El titulo y el ISBN son obligatorios.");
        }
        if (libro.AutorId <= 0)
        {
            throw new ReglaNegocioException("Debe seleccionar un autor.");
        }
        if (libro.Ejemplares < 0)
        {
            throw new ReglaNegocioException("La cantidad de ejemplares no puede ser negativa.");
        }
    }
}
