/* =====================================================================
   BibliotecaDB - Lab07 DesAEA
   Script completo: base de datos, tablas, procedimientos almacenados
   y datos de prueba.
   ===================================================================== */

IF DB_ID(N'BibliotecaDB') IS NULL
BEGIN
    CREATE DATABASE BibliotecaDB;
END
GO

USE BibliotecaDB;
GO

/* ---------------------------------------------------------------------
   1. TABLAS
   --------------------------------------------------------------------- */

IF OBJECT_ID(N'dbo.DetallePrestamo', N'U') IS NOT NULL DROP TABLE dbo.DetallePrestamo;
IF OBJECT_ID(N'dbo.Prestamos', N'U') IS NOT NULL DROP TABLE dbo.Prestamos;
IF OBJECT_ID(N'dbo.Libros', N'U') IS NOT NULL DROP TABLE dbo.Libros;
IF OBJECT_ID(N'dbo.Socios', N'U') IS NOT NULL DROP TABLE dbo.Socios;
IF OBJECT_ID(N'dbo.Autores', N'U') IS NOT NULL DROP TABLE dbo.Autores;
GO

CREATE TABLE dbo.Autores
(
    AutorId       INT IDENTITY(1,1) PRIMARY KEY,
    Nombre        NVARCHAR(150)   NOT NULL,
    Nacionalidad  NVARCHAR(100)   NOT NULL,
    Activo        BIT             NOT NULL CONSTRAINT DF_Autores_Activo DEFAULT (1)
);
GO

CREATE TABLE dbo.Libros
(
    LibroId     INT IDENTITY(1,1) PRIMARY KEY,
    Titulo      NVARCHAR(200)   NOT NULL,
    ISBN        NVARCHAR(20)    NOT NULL,
    AutorId     INT             NOT NULL,
    Ejemplares  INT             NOT NULL,
    Activo      BIT             NOT NULL CONSTRAINT DF_Libros_Activo DEFAULT (1),
    CONSTRAINT UQ_Libros_ISBN UNIQUE (ISBN),
    CONSTRAINT FK_Libros_Autores FOREIGN KEY (AutorId) REFERENCES dbo.Autores (AutorId),
    CONSTRAINT CK_Libros_Ejemplares CHECK (Ejemplares >= 0)
);
GO

CREATE TABLE dbo.Socios
(
    SocioId   INT IDENTITY(1,1) PRIMARY KEY,
    DNI       NVARCHAR(15)    NOT NULL,
    Nombre    NVARCHAR(150)   NOT NULL,
    Email     NVARCHAR(150)   NOT NULL,
    Activo    BIT             NOT NULL CONSTRAINT DF_Socios_Activo DEFAULT (1),
    CONSTRAINT UQ_Socios_DNI UNIQUE (DNI)
);
GO

CREATE TABLE dbo.Prestamos
(
    PrestamoId     INT IDENTITY(1,1) PRIMARY KEY,
    SocioId        INT             NOT NULL,
    FechaPrestamo  DATETIME2(0)    NOT NULL,
    FechaLimite    DATETIME2(0)    NOT NULL,
    Estado         NVARCHAR(20)    NOT NULL CONSTRAINT DF_Prestamos_Estado DEFAULT (N'Pendiente'),
    CONSTRAINT FK_Prestamos_Socios FOREIGN KEY (SocioId) REFERENCES dbo.Socios (SocioId),
    CONSTRAINT CK_Prestamos_Estado CHECK (Estado IN (N'Pendiente', N'Devuelto'))
);
GO

CREATE TABLE dbo.DetallePrestamo
(
    PrestamoId       INT             NOT NULL,
    LibroId          INT             NOT NULL,
    FechaDevolucion  DATETIME2(0)    NULL,
    CONSTRAINT PK_DetallePrestamo PRIMARY KEY (PrestamoId, LibroId),
    CONSTRAINT FK_DetallePrestamo_Prestamos FOREIGN KEY (PrestamoId) REFERENCES dbo.Prestamos (PrestamoId),
    CONSTRAINT FK_DetallePrestamo_Libros FOREIGN KEY (LibroId) REFERENCES dbo.Libros (LibroId)
);
GO

/* ---------------------------------------------------------------------
   2. PROCEDIMIENTOS ALMACENADOS
   --------------------------------------------------------------------- */

-- ===== Autores =====
CREATE OR ALTER PROCEDURE dbo.sp_Autor_Insertar
    @Nombre NVARCHAR(150),
    @Nacionalidad NVARCHAR(100),
    @AutorId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Autores (Nombre, Nacionalidad) VALUES (@Nombre, @Nacionalidad);
    SET @AutorId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Autor_Actualizar
    @AutorId INT,
    @Nombre NVARCHAR(150),
    @Nacionalidad NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Autores
       SET Nombre = @Nombre, Nacionalidad = @Nacionalidad
     WHERE AutorId = @AutorId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Autor_EliminarLogico
    @AutorId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Autores SET Activo = 0 WHERE AutorId = @AutorId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Autor_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT AutorId, Nombre, Nacionalidad, Activo FROM dbo.Autores ORDER BY Nombre;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Autor_BuscarPorId
    @AutorId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT AutorId, Nombre, Nacionalidad, Activo FROM dbo.Autores WHERE AutorId = @AutorId;
END
GO

-- ===== Libros =====
CREATE OR ALTER PROCEDURE dbo.sp_Libro_Insertar
    @Titulo NVARCHAR(200),
    @ISBN NVARCHAR(20),
    @AutorId INT,
    @Ejemplares INT,
    @LibroId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Libros (Titulo, ISBN, AutorId, Ejemplares)
    VALUES (@Titulo, @ISBN, @AutorId, @Ejemplares);
    SET @LibroId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Libro_Actualizar
    @LibroId INT,
    @Titulo NVARCHAR(200),
    @ISBN NVARCHAR(20),
    @AutorId INT,
    @Ejemplares INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Libros
       SET Titulo = @Titulo, ISBN = @ISBN, AutorId = @AutorId, Ejemplares = @Ejemplares
     WHERE LibroId = @LibroId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Libro_EliminarLogico
    @LibroId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Libros SET Activo = 0 WHERE LibroId = @LibroId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Libro_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT L.LibroId, L.Titulo, L.ISBN, L.AutorId, L.Ejemplares, L.Activo, A.Nombre AS NombreAutor
      FROM dbo.Libros L
      JOIN dbo.Autores A ON A.AutorId = L.AutorId
     ORDER BY L.Titulo;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Libro_BuscarPorId
    @LibroId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT L.LibroId, L.Titulo, L.ISBN, L.AutorId, L.Ejemplares, L.Activo, A.Nombre AS NombreAutor
      FROM dbo.Libros L
      JOIN dbo.Autores A ON A.AutorId = L.AutorId
     WHERE L.LibroId = @LibroId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Libro_BuscarPorIsbn
    @ISBN NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT LibroId, Titulo, ISBN, AutorId, Ejemplares, Activo
      FROM dbo.Libros
     WHERE ISBN = @ISBN;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Libro_BuscarPorTituloOAutor
    @Texto NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT L.LibroId, L.Titulo, L.ISBN, L.AutorId, L.Ejemplares, L.Activo, A.Nombre AS NombreAutor
      FROM dbo.Libros L
      JOIN dbo.Autores A ON A.AutorId = L.AutorId
     WHERE L.Titulo LIKE '%' + @Texto + '%'
        OR A.Nombre LIKE '%' + @Texto + '%'
     ORDER BY L.Titulo;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Libro_TienePrestamosPendientes
    @LibroId INT,
    @TienePendientes BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @TienePendientes = CASE WHEN EXISTS (
        SELECT 1
          FROM dbo.DetallePrestamo D
         WHERE D.LibroId = @LibroId AND D.FechaDevolucion IS NULL
    ) THEN 1 ELSE 0 END;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Libro_AjustarEjemplares
    @LibroId INT,
    @Delta INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Libros SET Ejemplares = Ejemplares + @Delta WHERE LibroId = @LibroId;
END
GO

-- ===== Socios =====
CREATE OR ALTER PROCEDURE dbo.sp_Socio_Insertar
    @DNI NVARCHAR(15),
    @Nombre NVARCHAR(150),
    @Email NVARCHAR(150),
    @SocioId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Socios (DNI, Nombre, Email) VALUES (@DNI, @Nombre, @Email);
    SET @SocioId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Socio_Actualizar
    @SocioId INT,
    @DNI NVARCHAR(15),
    @Nombre NVARCHAR(150),
    @Email NVARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Socios
       SET DNI = @DNI, Nombre = @Nombre, Email = @Email
     WHERE SocioId = @SocioId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Socio_EliminarLogico
    @SocioId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Socios SET Activo = 0 WHERE SocioId = @SocioId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Socio_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT SocioId, DNI, Nombre, Email, Activo FROM dbo.Socios ORDER BY Nombre;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Socio_BuscarPorId
    @SocioId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT SocioId, DNI, Nombre, Email, Activo FROM dbo.Socios WHERE SocioId = @SocioId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Socio_BuscarPorDni
    @DNI NVARCHAR(15)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT SocioId, DNI, Nombre, Email, Activo FROM dbo.Socios WHERE DNI = @DNI;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Socio_BuscarPorNombreODni
    @Texto NVARCHAR(150)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT SocioId, DNI, Nombre, Email, Activo
      FROM dbo.Socios
     WHERE Nombre LIKE '%' + @Texto + '%'
        OR DNI LIKE '%' + @Texto + '%'
     ORDER BY Nombre;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Socio_TienePrestamosPendientes
    @SocioId INT,
    @TienePendientes BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @TienePendientes = CASE WHEN EXISTS (
        SELECT 1
          FROM dbo.Prestamos P
         WHERE P.SocioId = @SocioId AND P.Estado = N'Pendiente'
    ) THEN 1 ELSE 0 END;
END
GO

-- ===== Prestamos / DetallePrestamo =====
CREATE OR ALTER PROCEDURE dbo.sp_Prestamo_ContarPendientesPorSocio
    @SocioId INT,
    @Cantidad INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT @Cantidad = COUNT(*)
      FROM dbo.Prestamos P
      JOIN dbo.DetallePrestamo D ON D.PrestamoId = P.PrestamoId
     WHERE P.SocioId = @SocioId AND D.FechaDevolucion IS NULL;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Prestamo_Insertar
    @SocioId INT,
    @FechaPrestamo DATETIME2(0),
    @FechaLimite DATETIME2(0),
    @PrestamoId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
    VALUES (@SocioId, @FechaPrestamo, @FechaLimite, N'Pendiente');
    SET @PrestamoId = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_DetallePrestamo_Insertar
    @PrestamoId INT,
    @LibroId INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.DetallePrestamo (PrestamoId, LibroId, FechaDevolucion)
    VALUES (@PrestamoId, @LibroId, NULL);
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Prestamo_RegistrarDevolucion
    @PrestamoId INT,
    @LibroId INT,
    @FechaDevolucion DATETIME2(0)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.DetallePrestamo
       SET FechaDevolucion = @FechaDevolucion
     WHERE PrestamoId = @PrestamoId AND LibroId = @LibroId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Prestamo_ActualizarEstado
    @PrestamoId INT,
    @Estado NVARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.Prestamos SET Estado = @Estado WHERE PrestamoId = @PrestamoId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Prestamo_QuedanPendientes
    @PrestamoId INT,
    @QuedanPendientes BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @QuedanPendientes = CASE WHEN EXISTS (
        SELECT 1 FROM dbo.DetallePrestamo WHERE PrestamoId = @PrestamoId AND FechaDevolucion IS NULL
    ) THEN 1 ELSE 0 END;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_Prestamo_BuscarPorId
    @PrestamoId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT PrestamoId, SocioId, FechaPrestamo, FechaLimite, Estado
      FROM dbo.Prestamos
     WHERE PrestamoId = @PrestamoId;
END
GO

CREATE OR ALTER PROCEDURE dbo.sp_DetallePrestamo_ListarPorPrestamo
    @PrestamoId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT D.PrestamoId, D.LibroId, D.FechaDevolucion, L.Titulo AS TituloLibro
      FROM dbo.DetallePrestamo D
      JOIN dbo.Libros L ON L.LibroId = D.LibroId
     WHERE D.PrestamoId = @PrestamoId;
END
GO

-- Reporte de prestamos por intervalo de fechas (INNER JOIN de las 4 tablas).
CREATE OR ALTER PROCEDURE dbo.sp_Prestamo_ReportePorRangoFechas
    @FechaInicio DATE,
    @FechaFin DATE
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        P.PrestamoId,
        S.Nombre      AS NombreSocio,
        L.Titulo      AS TituloLibro,
        P.FechaLimite,
        P.Estado
    FROM dbo.Prestamos P
    INNER JOIN dbo.DetallePrestamo D ON D.PrestamoId = P.PrestamoId
    INNER JOIN dbo.Libros L          ON L.LibroId = D.LibroId
    INNER JOIN dbo.Socios S          ON S.SocioId = P.SocioId
    WHERE P.FechaPrestamo >= @FechaInicio AND P.FechaPrestamo < DATEADD(DAY, 1, @FechaFin)
    ORDER BY P.FechaPrestamo, P.PrestamoId;
END
GO

/* ---------------------------------------------------------------------
   3. DATOS DE PRUEBA
   --------------------------------------------------------------------- */

-- 8 autores
INSERT INTO dbo.Autores (Nombre, Nacionalidad) VALUES
(N'Gabriel Garcia Marquez', N'Colombiana'),
(N'Mario Vargas Llosa', N'Peruana'),
(N'Isabel Allende', N'Chilena'),
(N'Jorge Luis Borges', N'Argentina'),
(N'Julio Cortazar', N'Argentina'),
(N'J.K. Rowling', N'Britanica'),
(N'George Orwell', N'Britanica'),
(N'Haruki Murakami', N'Japonesa');
GO

-- 20 libros (Ejemplares variados; ultimos 3 se dejan sin stock a proposito)
INSERT INTO dbo.Libros (Titulo, ISBN, AutorId, Ejemplares) VALUES
(N'Cien Anios de Soledad',            N'978-0307474728', 1, 5),
(N'El Amor en los Tiempos del Colera', N'978-0307389732', 1, 4),
(N'Cronica de una Muerte Anunciada',  N'978-1400034956', 1, 3),
(N'La Ciudad y los Perros',           N'978-8420471839', 2, 4),
(N'La Casa Verde',                    N'978-8420471846', 2, 2),
(N'Conversacion en la Catedral',      N'978-8420471853', 2, 3),
(N'La Casa de los Espiritus',         N'978-0525433477', 3, 5),
(N'Paula',                            N'978-0060175537', 3, 2),
(N'Eva Luna',                         N'978-0553383805', 3, 3),
(N'Ficciones',                        N'978-0802130303', 4, 4),
(N'El Aleph',                         N'978-0142437889', 4, 3),
(N'Rayuela',                          N'978-8437604572', 5, 4),
(N'Bestiario',                        N'978-8420633103', 5, 2),
(N'Harry Potter y la Piedra Filosofal', N'978-8478884452', 6, 6),
(N'Harry Potter y la Camara Secreta', N'978-8478884957', 6, 5),
(N'1984',                             N'978-0451524935', 7, 5),
(N'Rebelion en la Granja',            N'978-0451526342', 7, 4),
(N'Tokio Blues',                      N'978-8483835037', 8, 3),
(N'Kafka en la Orilla',               N'978-8483832224', 8, 0),
(N'1Q84',                             N'978-8483835822', 8, 0);
GO

-- 10 socios
INSERT INTO dbo.Socios (DNI, Nombre, Email) VALUES
(N'71234561', N'Ana Torres',       N'ana.torres@correo.com'),
(N'71234562', N'Luis Ramirez',     N'luis.ramirez@correo.com'),
(N'71234563', N'Maria Fernandez',  N'maria.fernandez@correo.com'),
(N'71234564', N'Carlos Diaz',      N'carlos.diaz@correo.com'),
(N'71234565', N'Sofia Rojas',      N'sofia.rojas@correo.com'),
(N'71234566', N'Diego Vargas',     N'diego.vargas@correo.com'),
(N'71234567', N'Lucia Mendoza',    N'lucia.mendoza@correo.com'),
(N'71234568', N'Pedro Castillo',   N'pedro.castillo@correo.com'),
(N'71234569', N'Valeria Chavez',   N'valeria.chavez@correo.com'),
(N'71234570', N'Jorge Salazar',    N'jorge.salazar@correo.com');
GO

-- 5 prestamos + detalle.
-- Prestamo 1 (Ana Torres, SocioId 1): 3 libros pendientes -> prueba de la regla de maximo 3.
DECLARE @P1 INT, @P2 INT, @P3 INT, @P4 INT, @P5 INT;

INSERT INTO dbo.Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
VALUES (1, DATEADD(DAY, -10, SYSDATETIME()), DATEADD(DAY, -3, SYSDATETIME()), N'Pendiente');
SET @P1 = SCOPE_IDENTITY();
INSERT INTO dbo.DetallePrestamo (PrestamoId, LibroId, FechaDevolucion) VALUES
(@P1, 1, NULL),
(@P1, 4, NULL),
(@P1, 7, NULL);

-- Prestamo 2 (Luis Ramirez, SocioId 2): ya devuelto por completo.
INSERT INTO dbo.Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
VALUES (2, DATEADD(DAY, -20, SYSDATETIME()), DATEADD(DAY, -13, SYSDATETIME()), N'Devuelto');
SET @P2 = SCOPE_IDENTITY();
INSERT INTO dbo.DetallePrestamo (PrestamoId, LibroId, FechaDevolucion) VALUES
(@P2, 10, DATEADD(DAY, -14, SYSDATETIME()));

-- Prestamo 3 (Maria Fernandez, SocioId 3): pendiente, dentro de plazo.
INSERT INTO dbo.Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
VALUES (3, DATEADD(DAY, -2, SYSDATETIME()), DATEADD(DAY, 5, SYSDATETIME()), N'Pendiente');
SET @P3 = SCOPE_IDENTITY();
INSERT INTO dbo.DetallePrestamo (PrestamoId, LibroId, FechaDevolucion) VALUES
(@P3, 14, NULL);

-- Prestamo 4 (Carlos Diaz, SocioId 4): pendiente, con 2 libros.
INSERT INTO dbo.Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
VALUES (4, DATEADD(DAY, -5, SYSDATETIME()), DATEADD(DAY, 2, SYSDATETIME()), N'Pendiente');
SET @P4 = SCOPE_IDENTITY();
INSERT INTO dbo.DetallePrestamo (PrestamoId, LibroId, FechaDevolucion) VALUES
(@P4, 16, NULL),
(@P4, 17, NULL);

-- Prestamo 5 (Sofia Rojas, SocioId 5): devuelto con retraso (para probar multa).
INSERT INTO dbo.Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
VALUES (5, DATEADD(DAY, -15, SYSDATETIME()), DATEADD(DAY, -8, SYSDATETIME()), N'Devuelto');
SET @P5 = SCOPE_IDENTITY();
INSERT INTO dbo.DetallePrestamo (PrestamoId, LibroId, FechaDevolucion) VALUES
(@P5, 18, DATEADD(DAY, -4, SYSDATETIME()));
GO
