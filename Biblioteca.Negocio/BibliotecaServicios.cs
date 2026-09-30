using Biblioteca.Datos;

namespace Biblioteca.Negocio;

/// <summary>
/// Punto de composicion de la aplicacion. Permite que WPF reciba servicios de Negocio sin
/// conocer ni construir clases de la capa de Datos.
/// </summary>
public sealed class BibliotecaServicios
{
    public LibroNegocio Libros { get; }
    public SocioNegocio Socios { get; }
    public PrestamoNegocio Prestamos { get; }

    public BibliotecaServicios()
    {
        var autores = new AutorRepositorio();
        var libros = new LibroRepositorio();
        var socios = new SocioRepositorio();
        var prestamos = new PrestamoRepositorio();
        var detalles = new DetallePrestamoRepositorio();

        Libros = new LibroNegocio(libros, autores);
        Socios = new SocioNegocio(socios);
        Prestamos = new PrestamoNegocio(prestamos, libros, socios, detalles);
    }
}
