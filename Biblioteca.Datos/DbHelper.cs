using System.Configuration;
using Microsoft.Data.SqlClient;

namespace Biblioteca.Datos;

/// <summary>
/// Punto unico de acceso a la cadena de conexion. La cadena vive en el App.config del
/// proyecto de inicio (Biblioteca.WPF): ConfigurationManager siempre resuelve el archivo
/// de configuracion del ejecutable en tiempo de ejecucion, sin importar en que ensamblado
/// corre este codigo.
/// </summary>
public static class DbHelper
{
    private const string ConnectionStringName = "BibliotecaDB";

    private static string ObtenerCadenaConexion()
    {
        var settings = ConfigurationManager.ConnectionStrings[ConnectionStringName];
        if (settings is null || string.IsNullOrWhiteSpace(settings.ConnectionString))
        {
            throw new InvalidOperationException(
                $"No se encontro la cadena de conexion '{ConnectionStringName}'. " +
                "Verifica el <connectionStrings> del App.config del proyecto de inicio (Biblioteca.WPF).");
        }
        return settings.ConnectionString;
    }

    public static SqlConnection CrearConexion() => new(ObtenerCadenaConexion());
}
