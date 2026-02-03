USE AgroPetech;
GO
-- PROCEDIMIENTOS PARA CURSO-ARCHIVO (RELACIÓN)
-- Procedimiento para obtener archivos de un curso
CREATE OR ALTER PROCEDURE GetArchivosPorCurso
    @iTransaccion VARCHAR(50),
    @iXML XML = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Respuesta VARCHAR(10) = 'Ok';
    DECLARE @Leyenda VARCHAR(200) = 'Ejecutado correctamente';
    DECLARE @CursoId INT;

    BEGIN TRY
        IF (@iTransaccion = 'ARCHIVOS_POR_CURSO')
        BEGIN
            -- Obtener ID del curso
            SET @CursoId = ISNULL(@iXML.value('(/CursoArchivo/CursoId)[1]', 'INT'), 0);
            
            -- Devolver archivos asociados al curso
            SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;
            
            SELECT 
                a.id,
                a.nombre,
                a.tipo,
                a.tamano,
                a.descripcion,
                a.usuario,
                a.estado,
                a.fechaSubida
            FROM Archivo a
            INNER JOIN CursoArchivo ca ON a.id = ca.archivoId
            WHERE ca.cursoId = @CursoId
            ORDER BY a.fechaSubida DESC;
        END
        ELSE
        BEGIN
            SET @Respuesta = 'Error';
            SET @Leyenda = 'Transacción no válida: ' + @iTransaccion;
            
            SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;
        END
    END TRY
    BEGIN CATCH
        SET @Respuesta = 'Error';
        SET @Leyenda = 'Error en la consulta: ' + ERROR_MESSAGE();
        
        SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;
    END CATCH
END;
GO

-- Procedimiento para agregar archivo a curso
CREATE OR ALTER PROCEDURE SetCursoArchivo
    @iTransaccion VARCHAR(50),
    @iXML XML = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Respuesta VARCHAR(10);
    DECLARE @Leyenda VARCHAR(200);
    DECLARE @CursoId INT;
    DECLARE @ArchivoId INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- AGREGAR ARCHIVO A CURSO
        IF (@iTransaccion = 'AGREGAR_ARCHIVO_CURSO')
        BEGIN
            SET @CursoId = ISNULL(@iXML.value('(/CursoArchivo/CursoId)[1]', 'INT'), 0);
            SET @ArchivoId = ISNULL(@iXML.value('(/CursoArchivo/ArchivoId)[1]', 'INT'), 0);

            -- Verificar que el curso existe
            IF NOT EXISTS (SELECT 1 FROM Curso WHERE id = @CursoId)
            BEGIN
                SET @Respuesta = 'Error';
                SET @Leyenda = 'El curso no existe';
            END
            ELSE IF NOT EXISTS (SELECT 1 FROM Archivo WHERE id = @ArchivoId)
            BEGIN
                SET @Respuesta = 'Error';
                SET @Leyenda = 'El archivo no existe';
            END
            ELSE IF EXISTS (SELECT 1 FROM CursoArchivo WHERE cursoId = @CursoId AND archivoId = @ArchivoId)
            BEGIN
                SET @Respuesta = 'Error';
                SET @Leyenda = 'El archivo ya está asociado a este curso';
            END
            ELSE
            BEGIN
                -- Insertar la relación
                INSERT INTO CursoArchivo (cursoId, archivoId)
                VALUES (@CursoId, @ArchivoId);
                
                SET @Respuesta = 'Ok';
                SET @Leyenda = 'Archivo agregado al curso correctamente';
            END
        END

        -- ELIMINAR ARCHIVO DE CURSO
        ELSE IF (@iTransaccion = 'ELIMINAR_ARCHIVO_CURSO')
        BEGIN
            SET @CursoId = ISNULL(@iXML.value('(/CursoArchivo/CursoId)[1]', 'INT'), 0);
            SET @ArchivoId = ISNULL(@iXML.value('(/CursoArchivo/ArchivoId)[1]', 'INT'), 0);

            DELETE FROM CursoArchivo 
            WHERE cursoId = @CursoId AND archivoId = @ArchivoId;
            
            SET @Respuesta = 'Ok';
            SET @Leyenda = 'Archivo eliminado del curso correctamente';
        END

        -- ELIMINAR TODOS LOS ARCHIVOS DE UN CURSO
        ELSE IF (@iTransaccion = 'ELIMINAR_TODOS_ARCHIVOS_CURSO')
        BEGIN
            SET @CursoId = ISNULL(@iXML.value('(/CursoArchivo/CursoId)[1]', 'INT'), 0);

            DELETE FROM CursoArchivo WHERE cursoId = @CursoId;
            
            SET @Respuesta = 'Ok';
            SET @Leyenda = 'Todos los archivos eliminados del curso';
        END

        ELSE
        BEGIN
            SET @Respuesta = 'Error';
            SET @Leyenda = 'Transacción no válida: ' + @iTransaccion;
        END

        COMMIT;
        SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;

    END TRY
    BEGIN CATCH
        ROLLBACK;
        SET @Respuesta = 'Error';
        SET @Leyenda = 'Error en la operación: ' + ERROR_MESSAGE();
        
        SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;
    END CATCH
END;
GO
-- Ordenamiento por fechaSubida en GetArchivosPorCurso
CREATE NONCLUSTERED INDEX IX_Archivo_FechaSubida
ON Archivo (fechaSubida DESC)
INCLUDE (nombre, tipo, tamano, descripcion, usuario, estado);
GO
-- JOIN y WHERE por cursoId (GetArchivosPorCurso)
CREATE NONCLUSTERED INDEX IX_CursoArchivo_CursoId
ON CursoArchivo (cursoId)
INCLUDE (archivoId);
GO
-- Validación EXISTS y DELETE por cursoId + archivoId
CREATE NONCLUSTERED INDEX IX_CursoArchivo_CursoId_ArchivoId
ON CursoArchivo (cursoId, archivoId);