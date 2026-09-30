# BibliotecaDB Lab 07

Aplicación WPF organizada en cuatro capas para administrar libros, socios, préstamos,
devoluciones y reportes por fechas.

## Preparación

1. Ejecute `sql/BibliotecaDB.sql` en SQL Server. El script crea `BibliotecaDB`, sus tablas,
   procedimientos almacenados y los datos de prueba solicitados.
2. Revise la cadena `BibliotecaDB` de `Biblioteca.WPF/App.config`. La configuración incluida
   usa la instancia predeterminada `localhost` con autenticación de Windows.
3. Abra `Lab07.slnx`, establezca `Biblioteca.WPF` como proyecto de inicio y ejecute la
   aplicación.

## Reglas implementadas en Negocio

`LibroNegocio` valida los datos obligatorios, la cantidad no negativa, la unicidad del ISBN
y que un libro no tenga préstamos pendientes antes de darlo de baja.

`SocioNegocio` valida los datos obligatorios, el formato básico del correo, la unicidad del
DNI y que un socio no tenga préstamos pendientes antes de darlo de baja.

`PrestamoNegocio` comprueba que se seleccione al menos un libro, que no se repitan libros,
que la fecha límite sea válida, que el socio y los libros estén activos, que exista stock y
que el socio no supere tres libros pendientes. En la devolución comprueba la pertenencia y
el estado del detalle, decide cuándo el préstamo queda totalmente devuelto y calcula la
multa de S/ 1.50 por día de retraso.

Estas reglas pertenecen a Negocio porque expresan las políticas de la biblioteca. La capa
Datos solo ejecuta operaciones parametrizadas y transacciones; WPF captura la excepción de
negocio y presenta el mensaje sin conocer `SqlClient` ni repositorios.

## Operaciones transaccionales

El alta de un préstamo guarda cabecera y detalles y descuenta los ejemplares en una sola
transacción. La devolución registra la fecha, repone el ejemplar y actualiza el estado del
préstamo en otra transacción. Cualquier fallo provoca rollback.

## Observaciones y conclusiones

La interfaz ejecuta todas las cargas mediante `async` y `await`; no utiliza `.Result` ni
`.Wait()`. El reporte se obtiene mediante el procedimiento almacenado que realiza `INNER
JOIN` entre `Prestamos`, `DetallePrestamo`, `Libros` y `Socios`.

La interfaz `ILibroRepositorio` se ubica en Datos para mantener las referencias obligatorias
del enunciado y evitar una dependencia circular entre Negocio y Datos. `LibroNegocio` la
recibe por constructor, por lo que conserva la inyección solicitada en el punto extra.
