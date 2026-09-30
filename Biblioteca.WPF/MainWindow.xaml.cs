using System.Windows;
using System.Windows.Controls;
using Biblioteca.Entidades;
using Biblioteca.Negocio;

namespace Biblioteca.WPF;

public partial class MainWindow : Window
{
    private readonly BibliotecaServicios _servicios = new();
    private int _libroSeleccionadoId;
    private int _socioSeleccionadoId;
    private bool _libroSeleccionadoActivo = true;
    private bool _socioSeleccionadoActivo = true;

    public MainWindow()
    {
        InitializeComponent();
        var hoy = DateTime.Today;
        DpFechaPrestamo.SelectedDate = hoy;
        DpFechaLimite.SelectedDate = hoy.AddDays(7);
        DpFechaDevolucion.SelectedDate = hoy;
        DpReporteInicio.SelectedDate = hoy.AddMonths(-1);
        DpReporteFin.SelectedDate = hoy;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await EjecutarSeguroAsync(CargarTodoAsync);
    }

    private async Task CargarTodoAsync()
    {
        var autores = await _servicios.Libros.ListarAutoresAsync();
        CmbLibroAutor.ItemsSource = autores.Where(a => a.Activo).ToList();
        await CargarLibrosAsync();
        await CargarSociosAsync();
        await CargarCatalogosPrestamoAsync();
        LblEstado.Text = "Datos cargados correctamente.";
    }

    private async Task CargarLibrosAsync()
    {
        DgLibros.ItemsSource = await _servicios.Libros.ListarAsync();
    }

    private async Task CargarSociosAsync()
    {
        DgSocios.ItemsSource = await _servicios.Socios.ListarAsync();
    }

    private async Task CargarCatalogosPrestamoAsync()
    {
        var socios = await _servicios.Socios.ListarAsync();
        var libros = await _servicios.Libros.ListarAsync();
        CmbPrestamoSocio.ItemsSource = socios.Where(s => s.Activo).ToList();
        DgPrestamoLibros.ItemsSource = libros.Where(l => l.Activo && l.Ejemplares > 0).ToList();
    }

    private async Task EjecutarSeguroAsync(Func<Task> accion)
    {
        try
        {
            IsEnabled = false;
            LblEstado.Text = "Procesando...";
            await accion();
        }
        catch (ReglaNegocioException ex)
        {
            LblEstado.Text = ex.Message;
            MessageBox.Show(ex.Message, "Regla de negocio", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            LblEstado.Text = "No se pudo completar la operación.";
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsEnabled = true;
        }
    }

    private void DgLibros_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DgLibros.SelectedItem is not Libro libro) return;
        _libroSeleccionadoId = libro.LibroId;
        _libroSeleccionadoActivo = libro.Activo;
        TxtLibroTitulo.Text = libro.Titulo;
        TxtLibroIsbn.Text = libro.ISBN;
        CmbLibroAutor.SelectedValue = libro.AutorId;
        TxtLibroEjemplares.Text = libro.Ejemplares.ToString();
    }

    private void BtnNuevoLibro_Click(object sender, RoutedEventArgs e) => LimpiarLibro();

    private async void BtnGuardarLibro_Click(object sender, RoutedEventArgs e)
    {
        await EjecutarSeguroAsync(async () =>
        {
            if (!int.TryParse(TxtLibroEjemplares.Text, out var ejemplares))
                throw new ReglaNegocioException("Ingrese una cantidad de ejemplares válida.");
            if (CmbLibroAutor.SelectedValue is not int autorId)
                throw new ReglaNegocioException("Debe seleccionar un autor.");

            var libro = new Libro
            {
                LibroId = _libroSeleccionadoId,
                Titulo = TxtLibroTitulo.Text.Trim(),
                ISBN = TxtLibroIsbn.Text.Trim(),
                AutorId = autorId,
                Ejemplares = ejemplares,
                Activo = _libroSeleccionadoActivo
            };

            if (libro.LibroId == 0)
                await _servicios.Libros.InsertarAsync(libro);
            else
                await _servicios.Libros.ActualizarAsync(libro);

            await CargarLibrosAsync();
            await CargarCatalogosPrestamoAsync();
            LimpiarLibro();
            LblEstado.Text = "Libro guardado correctamente.";
        });
    }

    private async void BtnEliminarLibro_Click(object sender, RoutedEventArgs e)
    {
        if (_libroSeleccionadoId == 0) return;
        await EjecutarSeguroAsync(async () =>
        {
            await _servicios.Libros.EliminarLogicoAsync(_libroSeleccionadoId);
            await CargarLibrosAsync();
            await CargarCatalogosPrestamoAsync();
            LimpiarLibro();
            LblEstado.Text = "Libro dado de baja correctamente.";
        });
    }

    private async void BtnBuscarLibro_Click(object sender, RoutedEventArgs e)
    {
        await EjecutarSeguroAsync(async () =>
        {
            DgLibros.ItemsSource = await _servicios.Libros.BuscarPorTituloOAutorAsync(TxtBuscarLibro.Text.Trim());
            LblEstado.Text = "Búsqueda de libros completada.";
        });
    }

    private async void BtnListarLibros_Click(object sender, RoutedEventArgs e)
    {
        await EjecutarSeguroAsync(CargarLibrosAsync);
    }

    private void LimpiarLibro()
    {
        _libroSeleccionadoId = 0;
        _libroSeleccionadoActivo = true;
        DgLibros.SelectedItem = null;
        TxtLibroTitulo.Clear();
        TxtLibroIsbn.Clear();
        CmbLibroAutor.SelectedIndex = -1;
        TxtLibroEjemplares.Text = "0";
    }

    private void DgSocios_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DgSocios.SelectedItem is not Socio socio) return;
        _socioSeleccionadoId = socio.SocioId;
        _socioSeleccionadoActivo = socio.Activo;
        TxtSocioDni.Text = socio.DNI;
        TxtSocioNombre.Text = socio.Nombre;
        TxtSocioEmail.Text = socio.Email;
    }

    private void BtnNuevoSocio_Click(object sender, RoutedEventArgs e) => LimpiarSocio();

    private async void BtnGuardarSocio_Click(object sender, RoutedEventArgs e)
    {
        await EjecutarSeguroAsync(async () =>
        {
            var socio = new Socio
            {
                SocioId = _socioSeleccionadoId,
                DNI = TxtSocioDni.Text.Trim(),
                Nombre = TxtSocioNombre.Text.Trim(),
                Email = TxtSocioEmail.Text.Trim(),
                Activo = _socioSeleccionadoActivo
            };

            if (socio.SocioId == 0)
                await _servicios.Socios.InsertarAsync(socio);
            else
                await _servicios.Socios.ActualizarAsync(socio);

            await CargarSociosAsync();
            await CargarCatalogosPrestamoAsync();
            LimpiarSocio();
            LblEstado.Text = "Socio guardado correctamente.";
        });
    }

    private async void BtnEliminarSocio_Click(object sender, RoutedEventArgs e)
    {
        if (_socioSeleccionadoId == 0) return;
        await EjecutarSeguroAsync(async () =>
        {
            await _servicios.Socios.EliminarLogicoAsync(_socioSeleccionadoId);
            await CargarSociosAsync();
            await CargarCatalogosPrestamoAsync();
            LimpiarSocio();
            LblEstado.Text = "Socio dado de baja correctamente.";
        });
    }

    private async void BtnBuscarSocio_Click(object sender, RoutedEventArgs e)
    {
        await EjecutarSeguroAsync(async () =>
        {
            DgSocios.ItemsSource = await _servicios.Socios.BuscarPorNombreODniAsync(TxtBuscarSocio.Text.Trim());
            LblEstado.Text = "Búsqueda de socios completada.";
        });
    }

    private async void BtnListarSocios_Click(object sender, RoutedEventArgs e)
    {
        await EjecutarSeguroAsync(CargarSociosAsync);
    }

    private void LimpiarSocio()
    {
        _socioSeleccionadoId = 0;
        _socioSeleccionadoActivo = true;
        DgSocios.SelectedItem = null;
        TxtSocioDni.Clear();
        TxtSocioNombre.Clear();
        TxtSocioEmail.Clear();
    }

    private async void BtnActualizarPrestamo_Click(object sender, RoutedEventArgs e)
    {
        await EjecutarSeguroAsync(CargarCatalogosPrestamoAsync);
    }

    private async void BtnRegistrarPrestamo_Click(object sender, RoutedEventArgs e)
    {
        await EjecutarSeguroAsync(async () =>
        {
            if (CmbPrestamoSocio.SelectedValue is not int socioId)
                throw new ReglaNegocioException("Debe seleccionar un socio.");
            if (DpFechaPrestamo.SelectedDate is not DateTime fechaPrestamo ||
                DpFechaLimite.SelectedDate is not DateTime fechaLimite)
                throw new ReglaNegocioException("Debe indicar las fechas del préstamo.");

            var libroIds = DgPrestamoLibros.SelectedItems.Cast<Libro>().Select(l => l.LibroId).ToList();
            var prestamoId = await _servicios.Prestamos.RegistrarPrestamoAsync(
                socioId, libroIds, fechaPrestamo, fechaLimite);

            await CargarLibrosAsync();
            await CargarCatalogosPrestamoAsync();
            LblEstado.Text = $"Préstamo {prestamoId} registrado correctamente.";
            MessageBox.Show($"Préstamo N.° {prestamoId} registrado.", "Préstamo", MessageBoxButton.OK,
                MessageBoxImage.Information);
        });
    }

    private async void BtnBuscarPrestamo_Click(object sender, RoutedEventArgs e)
    {
        await EjecutarSeguroAsync(CargarDetallePrestamoAsync);
    }

    private async Task CargarDetallePrestamoAsync()
    {
        if (!int.TryParse(TxtPrestamoId.Text, out var prestamoId) || prestamoId <= 0)
            throw new ReglaNegocioException("Ingrese un número de préstamo válido.");

        var prestamo = await _servicios.Prestamos.BuscarPorIdAsync(prestamoId)
            ?? throw new ReglaNegocioException("El préstamo indicado no existe.");
        var detalles = await _servicios.Prestamos.ListarDetalleAsync(prestamoId);
        DgDetallePrestamo.ItemsSource = detalles;
        LblDatosPrestamo.Text =
            $"Préstamo {prestamo.PrestamoId} | Límite: {prestamo.FechaLimite:dd/MM/yyyy} | Estado: {prestamo.Estado}";
        LblMulta.Text = "Multa: S/ 0.00";
        LblEstado.Text = "Detalle del préstamo cargado.";
    }

    private async void BtnRegistrarDevolucion_Click(object sender, RoutedEventArgs e)
    {
        await EjecutarSeguroAsync(async () =>
        {
            if (!int.TryParse(TxtPrestamoId.Text, out var prestamoId))
                throw new ReglaNegocioException("Ingrese un número de préstamo válido.");
            if (DgDetallePrestamo.SelectedItem is not DetallePrestamo detalle)
                throw new ReglaNegocioException("Seleccione el libro que se va a devolver.");
            if (DpFechaDevolucion.SelectedDate is not DateTime fechaDevolucion)
                throw new ReglaNegocioException("Indique la fecha de devolución.");

            var multa = await _servicios.Prestamos.RegistrarDevolucionAsync(
                prestamoId, detalle.LibroId, fechaDevolucion);
            await CargarDetallePrestamoAsync();
            LblMulta.Text = $"Multa: S/ {multa:N2}";
            await CargarLibrosAsync();
            await CargarCatalogosPrestamoAsync();
            LblEstado.Text = "Devolución registrada correctamente.";
        });
    }

    private async void BtnGenerarReporte_Click(object sender, RoutedEventArgs e)
    {
        await EjecutarSeguroAsync(async () =>
        {
            if (DpReporteInicio.SelectedDate is not DateTime inicio ||
                DpReporteFin.SelectedDate is not DateTime fin)
                throw new ReglaNegocioException("Seleccione el intervalo del reporte.");
            if (fin.Date < inicio.Date)
                throw new ReglaNegocioException("La fecha final no puede ser anterior a la inicial.");

            DgReporte.ItemsSource = await _servicios.Prestamos.ReportePorRangoFechasAsync(inicio, fin);
            LblEstado.Text = "Reporte generado correctamente.";
        });
    }
}
