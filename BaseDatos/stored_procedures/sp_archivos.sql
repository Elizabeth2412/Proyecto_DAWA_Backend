USE AgroPetech;
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
            
            SET @Leyenda = 'B�squeda Exitosa';
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
            SET @Leyenda = 'Transacci�n no v�lida: ' + @iTransaccion;
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
    SET XACT_ABORT ON; -- Para rollback autom�tico en errores

    DECLARE @Respuesta VARCHAR(10);
    DECLARE @Leyenda VARCHAR(200);
    DECLARE @ArchivoId INT;
    DECLARE @StartTime DATETIME = GETDATE();

    BEGIN TRY
        BEGIN TRANSACTION TRX_ARCHIVO;

        -- INSERTAR ARCHIVO
        IF (@iTransaccion = 'INSERTAR_ARCHIVO')
        BEGIN
            -- Usar variables con tipos espec�ficos
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

            -- Verificar si el archivo ya existe (usando �ndice)
            IF EXISTS (SELECT 1 FROM Archivo WITH (NOLOCK) WHERE nombre = @nombreI AND usuario = @usuarioI)
            BEGIN
                SET @Respuesta = 'Error';
                SET @Leyenda = 'El archivo ya existe: ' + @nombreI;
            END
            ELSE
            BEGIN
                -- Insertar con TRY/CATCH espec�fico
                BEGIN TRY
                    INSERT INTO Archivo (nombre, tipo, tamano, contenido, descripcion, usuario, estado)
                    VALUES (@nombreI, @tipoI, @tamanoI, 
                            CASE WHEN @contenidoI != '' THEN CAST(@contenidoI AS VARBINARY(MAX)) ELSE NULL END,
                            @descripcionI, @usuarioI, @estadoI);

                    SET @ArchivoId = SCOPE_IDENTITY();
                    SET @Respuesta = 'Ok';
                    SET @Leyenda = 'Archivo insertado correctamente. ID: ' + CAST(@ArchivoId AS VARCHAR);
                    
                    -- Log de tiempo de ejecuci�n
                    DECLARE @EndTime DATETIME = GETDATE();
                    DECLARE @DurationMs INT = DATEDIFF(MILLISECOND, @StartTime, @EndTime);
                    PRINT 'Tiempo de inserci�n: ' + CAST(@DurationMs AS VARCHAR) + ' ms';
                    
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
            SET @Leyenda = 'Transacci�n no v�lida: ' + @iTransaccion;
        END

        COMMIT TRANSACTION TRX_ARCHIVO;
        
        -- Devolver resultado
        SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION TRX_ARCHIVO;

        SET @Respuesta = 'Error';
        SET @Leyenda = 'Error en la transacci�n ' + @iTransaccion + ': ' + ERROR_MESSAGE();
        
        SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;
    END CATCH
END;
GO
-- Indice para optimizaci�n de b�squeda de usuario y archivo 
CREATE INDEX IX_Archivo_Nombre_Usuario ON Archivo(nombre, usuario);

GO
SELECT * FROM Archivo;