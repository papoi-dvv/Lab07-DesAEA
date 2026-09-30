using Biblioteca.Datos;
using Biblioteca.Entidades;

namespace Biblioteca.Negocio;

public class SocioNegocio
{
    private readonly SocioRepositorio _socioRepositorio;

    public SocioNegocio(SocioRepositorio socioRepositorio)
    {
        _socioRepositorio = socioRepositorio;
    }

    public List<Socio> Listar() => _socioRepositorio.Listar();

    public Task<List<Socio>> ListarAsync(CancellationToken ct = default) => _socioRepositorio.ListarAsync(ct);

    public List<Socio> BuscarPorNombreODni(string texto) => _socioRepositorio.BuscarPorNombreODni(texto);

    public Task<List<Socio>> BuscarPorNombreODniAsync(string texto, CancellationToken ct = default) =>
        _socioRepositorio.BuscarPorNombreODniAsync(texto, ct);

    public int Insertar(Socio socio)
    {
        ValidarDniDuplicado(socio);
        return _socioRepositorio.Insertar(socio);
    }

    public async Task<int> InsertarAsync(Socio socio, CancellationToken ct = default)
    {
        ValidarCampos(socio);
        await ValidarDniDuplicadoAsync(socio, ct);
        return await _socioRepositorio.InsertarAsync(socio, ct);
    }

    public void Actualizar(Socio socio)
    {
        ValidarDniDuplicado(socio);
        _socioRepositorio.Actualizar(socio);
    }

    public async Task ActualizarAsync(Socio socio, CancellationToken ct = default)
    {
        ValidarCampos(socio);
        await ValidarDniDuplicadoAsync(socio, ct);
        await _socioRepositorio.ActualizarAsync(socio, ct);
    }

    public void EliminarLogico(int socioId)
    {
        if (_socioRepositorio.TienePrestamosPendientes(socioId))
        {
            throw new ReglaNegocioException("No se puede dar de baja un socio con prestamos pendientes.");
        }
        _socioRepositorio.EliminarLogico(socioId);
    }

    public async Task EliminarLogicoAsync(int socioId, CancellationToken ct = default)
    {
        if (await _socioRepositorio.TienePrestamosPendientesAsync(socioId, ct))
        {
            throw new ReglaNegocioException("No se puede dar de baja un socio con prestamos pendientes.");
        }
        await _socioRepositorio.EliminarLogicoAsync(socioId, ct);
    }

    private void ValidarDniDuplicado(Socio socio)
    {
        var existente = _socioRepositorio.BuscarPorDni(socio.DNI);
        if (existente is not null && existente.SocioId != socio.SocioId)
        {
            throw new ReglaNegocioException($"Ya existe un socio registrado con el DNI '{socio.DNI}'.");
        }
    }

    private async Task ValidarDniDuplicadoAsync(Socio socio, CancellationToken ct)
    {
        var existente = await _socioRepositorio.BuscarPorDniAsync(socio.DNI, ct);
        if (existente is not null && existente.SocioId != socio.SocioId)
        {
            throw new ReglaNegocioException($"Ya existe un socio registrado con el DNI '{socio.DNI}'.");
        }
    }

    private static void ValidarCampos(Socio socio)
    {
        if (string.IsNullOrWhiteSpace(socio.DNI) || string.IsNullOrWhiteSpace(socio.Nombre) ||
            string.IsNullOrWhiteSpace(socio.Email))
        {
            throw new ReglaNegocioException("El DNI, el nombre y el correo son obligatorios.");
        }
        if (!socio.Email.Contains('@'))
        {
            throw new ReglaNegocioException("Ingrese un correo electronico valido.");
        }
    }
}
