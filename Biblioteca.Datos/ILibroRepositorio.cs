using Biblioteca.Entidades;

namespace Biblioteca.Datos;

// Punto extra (#17): interfaz declarada junto al resto de Datos para que Negocio y Datos
// la compartan sin que Negocio dependa del tipo concreto LibroRepositorio.
public interface ILibroRepositorio
{
    int Insertar(Libro libro);
    Task<int> InsertarAsync(Libro libro, CancellationToken ct = default);

    void Actualizar(Libro libro);
    Task ActualizarAsync(Libro libro, CancellationToken ct = default);

    void EliminarLogico(int libroId);
    Task EliminarLogicoAsync(int libroId, CancellationToken ct = default);

    List<Libro> Listar();
    Task<List<Libro>> ListarAsync(CancellationToken ct = default);

    Libro? BuscarPorId(int libroId);
    Task<Libro?> BuscarPorIdAsync(int libroId, CancellationToken ct = default);

    Libro? BuscarPorIsbn(string isbn);
    Task<Libro?> BuscarPorIsbnAsync(string isbn, CancellationToken ct = default);

    List<Libro> BuscarPorTituloOAutor(string texto);
    Task<List<Libro>> BuscarPorTituloOAutorAsync(string texto, CancellationToken ct = default);

    bool TienePrestamosPendientes(int libroId);
    Task<bool> TienePrestamosPendientesAsync(int libroId, CancellationToken ct = default);

    void AjustarEjemplares(int libroId, int delta);
    Task AjustarEjemplaresAsync(int libroId, int delta, CancellationToken ct = default);
}
