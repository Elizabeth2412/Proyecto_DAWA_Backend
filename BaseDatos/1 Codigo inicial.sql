-- Base de datos para AgroPetech
USE AgroPetechDB;
GO

-- Tabla de Usuarios
CREATE TABLE Usuario (
    id INT IDENTITY(1,1) PRIMARY KEY,
    email VARCHAR(100) UNIQUE NOT NULL,
    password VARCHAR(100) NOT NULL,
    tipo VARCHAR(20) CHECK (tipo IN ('administrador', 'instructor', 'estudiante')),
    nombre VARCHAR(100) NOT NULL,
    apellido VARCHAR(100),
    edad INT
);
GO

-- Tabla de Cursos
CREATE TABLE Curso (
    id INT IDENTITY(1,1) PRIMARY KEY,
    titulo VARCHAR(200) NOT NULL,
    descripcion VARCHAR(1000),
    nivel VARCHAR(50),
    duracion INT,
    progreso INT DEFAULT 0,
    instructor VARCHAR(100) NOT NULL,
    fechaCreacion DATETIME DEFAULT GETDATE(),
    fechaActualizacion DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (instructor) REFERENCES Usuario(email)
);
GO

-- Tabla de Archivos
CREATE TABLE Archivo (
    id INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(200) NOT NULL,
    tipo VARCHAR(20) CHECK (tipo IN ('PDF', 'PPTX', 'DOCX')),
    tamano BIGINT NOT NULL,
    contenido VARBINARY(MAX),
    descripcion VARCHAR(500),
    usuario VARCHAR(100) NOT NULL,
    estado VARCHAR(20) CHECK (estado IN ('Disponible', 'NoDisponible')),
    fechaSubida DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (usuario) REFERENCES Usuario(email)
);
GO

-- Tabla intermedia para relación Curso-Archivo
CREATE TABLE CursoArchivo (
    cursoId INT,
    archivoId INT,
    PRIMARY KEY (cursoId, archivoId),
    FOREIGN KEY (cursoId) REFERENCES Curso(id) ON DELETE CASCADE,
    FOREIGN KEY (archivoId) REFERENCES Archivo(id) ON DELETE CASCADE
);
GO

-- Tabla de Evaluaciones
CREATE TABLE Evaluacion (
    id INT IDENTITY(1,1) PRIMARY KEY,
    titulo VARCHAR(200) NOT NULL,
    modulo VARCHAR(100),
    totalPreguntas INT DEFAULT 0,
    duracion VARCHAR(50),
    fechaCreacion DATETIME DEFAULT GETDATE(),
    estado VARCHAR(20) CHECK (estado IN ('Activa', 'Inactiva'))
);
GO

-- Tabla de Preguntas
CREATE TABLE Pregunta (
    id INT IDENTITY(1,1) PRIMARY KEY,
    evaluacionId INT,
    texto VARCHAR(500) NOT NULL,
    FOREIGN KEY (evaluacionId) REFERENCES Evaluacion(id) ON DELETE CASCADE
);
GO

-- Tabla de Opciones de Preguntas
CREATE TABLE Opcion (
    id INT IDENTITY(1,1) PRIMARY KEY,
    preguntaId INT,
    texto VARCHAR(200) NOT NULL,
    esCorrecta BIT DEFAULT 0,
    FOREIGN KEY (preguntaId) REFERENCES Pregunta(id) ON DELETE CASCADE
);
GO


-- Datos iniciales
INSERT INTO Usuario (email, password, tipo, nombre, apellido, edad) 
VALUES 
    ('elizabeth@gmail.com', 'admin123', 'administrador', 'Elizabeth', 'Franco', 20),
    ('leslie@gmail.com', 'instructor123', 'instructor', 'Leslie', 'Vera', 20),
    ('joshua@hotmail.com', 'estudiante123', 'estudiante', 'Joshúa', 'Castillo', 20),
    ('jonacas2000@outlook.com', '123456', 'estudiante', 'Jonathan', 'Castro', 20),
    ('juan@outlook.com', '123456', 'estudiante', 'Juan', 'Robles', 20);
GO
-- Procedimiento  para Usuario
CREATE OR ALTER PROCEDURE GetUsuario
    @iTransaccion VARCHAR(50),
    @iXML XML = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Respuesta VARCHAR(10) = 'Ok';
    DECLARE @Leyenda VARCHAR(200) = 'Ejecutado correctamente';
    DECLARE @ResultTable TABLE (
        id INT,
        email VARCHAR(100),
        tipo VARCHAR(20),
        nombre VARCHAR(100),
        apellido VARCHAR(100),
        edad INT
    );

    BEGIN TRY
        -- CONSULTAR TODOS LOS USUARIOS
        IF (@iTransaccion = 'CONSULTAR_USUARIO')
        BEGIN
            INSERT INTO @ResultTable
            SELECT 
                id,
                email,
                tipo,
                nombre,
                apellido,
                edad
            FROM Usuario;
            SET @Leyenda = 'Consulta Exitosa';
        END

        -- VALIDAR USUARIO (Login)
        ELSE IF (@iTransaccion = 'VALIDAR_USUARIO')
        BEGIN
            DECLARE @emailT VARCHAR(100) = @iXML.value('(/Usuario/Email)[1]', 'VARCHAR(100)');
            DECLARE @passwordT VARCHAR(100) = @iXML.value('(/Usuario/Password)[1]', 'VARCHAR(100)');

            INSERT INTO @ResultTable
            SELECT 
                id,
                email,
                tipo,
                nombre,
                apellido,
                edad
            FROM Usuario
            WHERE email = @emailT AND password = @passwordT;
            
            SET @Leyenda = 'Validación Exitosa';
        END

        -- BUSCAR USUARIO POR EMAIL
        ELSE IF (@iTransaccion = 'BUSCAR_USUARIO')
        BEGIN
            DECLARE @emailBuscar VARCHAR(100) = @iXML.value('(/Usuario/Email)[1]', 'VARCHAR(100)');

            INSERT INTO @ResultTable
            SELECT 
                id,
                email,
                tipo,
                nombre,
                apellido,
                edad
            FROM Usuario
            WHERE email = @emailBuscar;
            
            SET @Leyenda = 'Búsqueda Exitosa';
        END

        ELSE
        BEGIN
            SET @Respuesta = 'Error';
            SET @Leyenda = 'Transacción no válida: ' + @iTransaccion;
        END

        -- Devolver la tabla de resultados
        SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;
        SELECT * FROM @ResultTable;

    END TRY
    BEGIN CATCH
        SET @Respuesta = 'Error';
        SET @Leyenda = 'Error en la consulta: ' + ERROR_MESSAGE();
        
        SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;
        SELECT TOP 0 * FROM @ResultTable;
    END CATCH
END;
GO

-- Procedimiento  para operaciones CRUD de Usuario
CREATE OR ALTER PROCEDURE SetUsuario
    @iTransaccion VARCHAR(50),
    @iXML XML = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Respuesta VARCHAR(10);
    DECLARE @Leyenda VARCHAR(200);
    DECLARE @UsuarioId INT;

    BEGIN TRY
        BEGIN TRANSACTION TRX_USUARIO;

        -- INSERTAR USUARIO
        IF (@iTransaccion = 'INSERTAR_USUARIO')
        BEGIN
            DECLARE @emailI VARCHAR(100) = LTRIM(RTRIM(ISNULL(@iXML.value('(/Usuario/Email)[1]', 'VARCHAR(100)'), '')));
            DECLARE @passwordI VARCHAR(100) = LTRIM(RTRIM(ISNULL(@iXML.value('(/Usuario/Password)[1]', 'VARCHAR(100)'), '')));
            DECLARE @tipoI VARCHAR(20) = LTRIM(RTRIM(ISNULL(@iXML.value('(/Usuario/Tipo)[1]', 'VARCHAR(20)'), 'estudiante')));
            DECLARE @nombreI VARCHAR(100) = LTRIM(RTRIM(ISNULL(@iXML.value('(/Usuario/Nombre)[1]', 'VARCHAR(100)'), '')));
            DECLARE @apellidoI VARCHAR(100) = LTRIM(RTRIM(ISNULL(@iXML.value('(/Usuario/Apellido)[1]', 'VARCHAR(100)'), '')));
            DECLARE @edadI INT = ISNULL(@iXML.value('(/Usuario/Edad)[1]', 'INT'), 0);

            -- Verificar si el usuario ya existe
            IF EXISTS (SELECT 1 FROM Usuario WHERE email = @emailI)
            BEGIN
                SET @Respuesta = 'Error';
                SET @Leyenda = 'El usuario ya existe: ' + @emailI;
            END
            ELSE
            BEGIN
                INSERT INTO Usuario (email, password, tipo, nombre, apellido, edad)
                VALUES (@emailI, @passwordI, @tipoI, @nombreI, @apellidoI, @edadI);

                SET @UsuarioId = SCOPE_IDENTITY();
                SET @Respuesta = 'Ok';
                SET @Leyenda = 'Usuario registrado correctamente: ' + @emailI;
            END
        END

        -- ACTUALIZAR USUARIO
        ELSE IF (@iTransaccion = 'ACTUALIZAR_USUARIO')
        BEGIN
            DECLARE @emailU VARCHAR(100) = LTRIM(RTRIM(ISNULL(@iXML.value('(/Usuario/Email)[1]', 'VARCHAR(100)'), '')));
            DECLARE @nombreU VARCHAR(100) = LTRIM(RTRIM(ISNULL(@iXML.value('(/Usuario/Nombre)[1]', 'VARCHAR(100)'), '')));
            DECLARE @apellidoU VARCHAR(100) = LTRIM(RTRIM(ISNULL(@iXML.value('(/Usuario/Apellido)[1]', 'VARCHAR(100)'), '')));
            DECLARE @edadU INT = ISNULL(@iXML.value('(/Usuario/Edad)[1]', 'INT'), 0);
            DECLARE @tipoU VARCHAR(20) = LTRIM(RTRIM(ISNULL(@iXML.value('(/Usuario/Tipo)[1]', 'VARCHAR(20)'), 'estudiante')));

            UPDATE Usuario
            SET nombre = @nombreU,
                apellido = @apellidoU,
                edad = @edadU,
                tipo = @tipoU
            WHERE email = @emailU;

            SET @Respuesta = 'Ok';
            SET @Leyenda = 'Usuario actualizado correctamente: ' + @emailU;
        END

        -- ELIMINAR USUARIO
        ELSE IF (@iTransaccion = 'ELIMINAR_USUARIO')
        BEGIN
            DECLARE @emailE VARCHAR(100) = LTRIM(RTRIM(ISNULL(@iXML.value('(/Usuario/Email)[1]', 'VARCHAR(100)'), '')));

            DELETE FROM Usuario WHERE email = @emailE;

            SET @Respuesta = 'Ok';
            SET @Leyenda = 'Usuario eliminado correctamente: ' + @emailE;
        END

        -- CAMBIAR PASSWORD
        ELSE IF (@iTransaccion = 'CAMBIAR_PASSWORD')
        BEGIN
            DECLARE @emailC VARCHAR(100) = LTRIM(RTRIM(ISNULL(@iXML.value('(/Usuario/Email)[1]', 'VARCHAR(100)'), '')));
            DECLARE @passwordC VARCHAR(100) = LTRIM(RTRIM(ISNULL(@iXML.value('(/Usuario/Password)[1]', 'VARCHAR(100)'), '')));

            UPDATE Usuario
            SET password = @passwordC
            WHERE email = @emailC;

            SET @Respuesta = 'Ok';
            SET @Leyenda = 'Contraseña actualizada para: ' + @emailC;
        END

        ELSE
        BEGIN
            SET @Respuesta = 'Error';
            SET @Leyenda = 'Transacción no válida: ' + @iTransaccion;
        END

        COMMIT TRANSACTION TRX_USUARIO;
        
        -- Devolver resultado
        SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION TRX_USUARIO;

        SET @Respuesta = 'Error';
        SET @Leyenda = 'Error en la transacción ' + @iTransaccion + ': ' + ERROR_MESSAGE();
        
        SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;
    END CATCH
END;
GO

-- Procedimiento  para Archivos
CREATE OR ALTER PROCEDURE GetArchivo
    @iTransaccion VARCHAR(50),
    @iXML XML = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Respuesta VARCHAR(10) = 'Ok';
    DECLARE @Leyenda VARCHAR(200) = 'Ejecutado correctamente';
    DECLARE @ResultTable TABLE (
        id INT,
        nombre VARCHAR(200),
        tipo VARCHAR(20),
        tamano BIGINT,
        contenido VARBINARY(MAX),
        descripcion VARCHAR(500),
        usuario VARCHAR(100),
        estado VARCHAR(20),
        fechaSubida DATETIME
    );

    BEGIN TRY
        -- CONSULTAR TODOS LOS ARCHIVOS (sin contenido para listado)
        IF (@iTransaccion = 'CONSULTAR_ARCHIVO')
        BEGIN
            INSERT INTO @ResultTable
            SELECT 
                id,
                nombre,
                tipo,
                tamano,
                NULL AS contenido, -- No enviamos contenido en listado general
                descripcion,
                usuario,
                estado,
                fechaSubida
            FROM Archivo;
            
            SET @Leyenda = 'Consulta Exitosa';
        END

        -- BUSCAR ARCHIVO POR ID (con contenido para descarga)
        ELSE IF (@iTransaccion = 'BUSCAR_ARCHIVO')
        BEGIN
            DECLARE @idBuscar INT = ISNULL(@iXML.value('(/Archivo/Id)[1]', 'INT'), 0);

            INSERT INTO @ResultTable
            SELECT 
                id,
                nombre,
                tipo,
                tamano,
                contenido,
                descripcion,
                usuario,
                estado,
                fechaSubida
            FROM Archivo
            WHERE id = @idBuscar;
            
            SET @Leyenda = 'Búsqueda Exitosa';
        END

        -- ARCHIVOS POR USUARIO
        ELSE IF (@iTransaccion = 'ARCHIVOS_POR_USUARIO')
        BEGIN
            DECLARE @usuarioB VARCHAR(100) = LTRIM(RTRIM(ISNULL(@iXML.value('(/Archivo/Usuario)[1]', 'VARCHAR(100)'), '')));

            INSERT INTO @ResultTable
            SELECT 
                id,
                nombre,
                tipo,
                tamano,
                NULL AS contenido,
                descripcion,
                usuario,
                estado,
                fechaSubida
            FROM Archivo
            WHERE usuario = @usuarioB;
            
            SET @Leyenda = 'Consulta Exitosa';
        END

        ELSE
        BEGIN
            SET @Respuesta = 'Error';
            SET @Leyenda = 'Transacción no válida: ' + @iTransaccion;
        END

        -- Devolver la tabla de resultados
        SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;
        SELECT * FROM @ResultTable;

    END TRY
    BEGIN CATCH
        SET @Respuesta = 'Error';
        SET @Leyenda = 'Error en la consulta: ' + ERROR_MESSAGE();
        
        SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;
        SELECT TOP 0 * FROM @ResultTable;
    END CATCH
END;
GO

-- Procedimiento  para operaciones CRUD de Archivos (con manejo de Base64)
CREATE OR ALTER PROCEDURE SetArchivo
    @iTransaccion VARCHAR(50),
    @iXML XML = NULL
AS
BEGIN
    SET XACT_ABORT ON; -- Para rollback automático en errores

    DECLARE @Respuesta VARCHAR(10);
    DECLARE @Leyenda VARCHAR(200);
    DECLARE @ArchivoId INT;
    DECLARE @StartTime DATETIME = GETDATE();

    BEGIN TRY
        BEGIN TRANSACTION TRX_ARCHIVO;

        -- INSERTAR ARCHIVO
        IF (@iTransaccion = 'INSERTAR_ARCHIVO')
        BEGIN
            -- Usar variables con tipos específicos
            DECLARE @nombreI VARCHAR(200);
            DECLARE @tipoI VARCHAR(20);
            DECLARE @tamanoI BIGINT;
            DECLARE @contenidoI VARCHAR(MAX);
            DECLARE @descripcionI VARCHAR(500);
            DECLARE @usuarioI VARCHAR(100);
            DECLARE @estadoI VARCHAR(20);

            -- Extraer valores del XML
            SELECT 
                @nombreI = LTRIM(RTRIM(ISNULL(@iXML.value('(/Archivo/Nombre)[1]', 'VARCHAR(200)'), ''))),
                @tipoI = LTRIM(RTRIM(ISNULL(@iXML.value('(/Archivo/Tipo)[1]', 'VARCHAR(20)'), 'PDF'))),
                @tamanoI = ISNULL(@iXML.value('(/Archivo/Tamano)[1]', 'BIGINT'), 0),
                @contenidoI = ISNULL(@iXML.value('(/Archivo/Contenido)[1]', 'VARCHAR(MAX)'), ''),
                @descripcionI = LTRIM(RTRIM(ISNULL(@iXML.value('(/Archivo/Descripcion)[1]', 'VARCHAR(500)'), ''))),
                @usuarioI = LTRIM(RTRIM(ISNULL(@iXML.value('(/Archivo/Usuario)[1]', 'VARCHAR(100)'), ''))),
                @estadoI = LTRIM(RTRIM(ISNULL(@iXML.value('(/Archivo/Estado)[1]', 'VARCHAR(20)'), 'Disponible')));

            -- Verificar si el archivo ya existe (usando índice)
            IF EXISTS (SELECT 1 FROM Archivo WITH (NOLOCK) WHERE nombre = @nombreI AND usuario = @usuarioI)
            BEGIN
                SET @Respuesta = 'Error';
                SET @Leyenda = 'El archivo ya existe: ' + @nombreI;
            END
            ELSE
            BEGIN
                -- Insertar con TRY/CATCH específico
                BEGIN TRY
                    INSERT INTO Archivo (nombre, tipo, tamano, contenido, descripcion, usuario, estado)
                    VALUES (@nombreI, @tipoI, @tamanoI, 
                            CASE WHEN @contenidoI != '' THEN CAST(@contenidoI AS VARBINARY(MAX)) ELSE NULL END,
                            @descripcionI, @usuarioI, @estadoI);

                    SET @ArchivoId = SCOPE_IDENTITY();
                    SET @Respuesta = 'Ok';
                    SET @Leyenda = 'Archivo insertado correctamente. ID: ' + CAST(@ArchivoId AS VARCHAR);
                    
                    -- Log de tiempo de ejecución
                    DECLARE @EndTime DATETIME = GETDATE();
                    DECLARE @DurationMs INT = DATEDIFF(MILLISECOND, @StartTime, @EndTime);
                    PRINT 'Tiempo de inserción: ' + CAST(@DurationMs AS VARCHAR) + ' ms';
                    
                END TRY
                BEGIN CATCH
                    SET @Respuesta = 'Error';
                    SET @Leyenda = 'Error al insertar archivo: ' + ERROR_MESSAGE();
                END CATCH
            END
        END

        -- ACTUALIZAR ARCHIVO
        ELSE IF (@iTransaccion = 'ACTUALIZAR_ARCHIVO')
        BEGIN
            DECLARE @idU INT = ISNULL(@iXML.value('(/Archivo/Id)[1]', 'INT'), 0);
            DECLARE @nombreU VARCHAR(200) = LTRIM(RTRIM(ISNULL(@iXML.value('(/Archivo/Nombre)[1]', 'VARCHAR(200)'), '')));
            DECLARE @descripcionU VARCHAR(500) = LTRIM(RTRIM(ISNULL(@iXML.value('(/Archivo/Descripcion)[1]', 'VARCHAR(500)'), '')));
            DECLARE @estadoU VARCHAR(20) = LTRIM(RTRIM(ISNULL(@iXML.value('(/Archivo/Estado)[1]', 'VARCHAR(20)'), 'Disponible')));

            UPDATE Archivo
            SET nombre = @nombreU,
                descripcion = @descripcionU,
                estado = @estadoU
            WHERE id = @idU;

            SET @Respuesta = 'Ok';
            SET @Leyenda = 'Archivo actualizado correctamente. ID: ' + CAST(@idU AS VARCHAR);
        END

        -- ELIMINAR ARCHIVO
        ELSE IF (@iTransaccion = 'ELIMINAR_ARCHIVO')
        BEGIN
            DECLARE @idE INT = ISNULL(@iXML.value('(/Archivo/Id)[1]', 'INT'), 0);

            DELETE FROM Archivo WHERE id = @idE;

            SET @Respuesta = 'Ok';
            SET @Leyenda = 'Archivo eliminado correctamente. ID: ' + CAST(@idE AS VARCHAR);
        END

        ELSE
        BEGIN
            SET @Respuesta = 'Error';
            SET @Leyenda = 'Transacción no válida: ' + @iTransaccion;
        END

        COMMIT TRANSACTION TRX_ARCHIVO;
        
        -- Devolver resultado
        SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION TRX_ARCHIVO;

        SET @Respuesta = 'Error';
        SET @Leyenda = 'Error en la transacción ' + @iTransaccion + ': ' + ERROR_MESSAGE();
        
        SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;
    END CATCH
END;
GO
-- Indice para optimización de búsqueda de usuario y archivo 
CREATE INDEX IX_Archivo_Nombre_Usuario ON Archivo(nombre, usuario);

GO
SELECT * FROM Usuario;
SELECT * FROM Archivo;