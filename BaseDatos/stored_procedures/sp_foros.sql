/* Stored Procedures - Foros */
USE AgroPetech;
GO
-- ===============================
-- GetPublicacionForo
-- ===============================
CREATE OR ALTER PROCEDURE [dbo].[GetPublicacionForo]
    @iTransaccion VARCHAR(50),
    @iXml XML = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Respuesta VARCHAR(10) = 'Ok';
    DECLARE @Leyenda VARCHAR(200) = 'Ejecutado correctamente';
    
    DECLARE @id BIGINT;
    DECLARE @usuarioId INT;
    DECLARE @textoBusqueda NVARCHAR(200);

    -- UNA SOLA tabla temporal para TODAS las transacciones
    DECLARE @Result TABLE (
        id BIGINT,
        titulo NVARCHAR(200),
        contenido NVARCHAR(MAX),
        urlImagen VARCHAR(500),
        usuarioId INT,
        parentId BIGINT,
        rootId BIGINT,
        nombreAutor NVARCHAR(202),
        fechaCreacion DATETIME,
        fechaModificacion DATETIME,
        usuarioModificacionId INT,
        numeroRespuestas INT
    );

    BEGIN TRY
        
        -- Posts raíz (Principales del foro)
        IF (@iTransaccion = 'CONSULTAR_POSTS')
        BEGIN
            INSERT INTO @Result
            SELECT 
                p.id,
                p.titulo,
                p.contenido,
                p.urlImagen,
                p.usuarioId,
                p.parentId,
                p.rootId,
                u.nombre + ' ' + u.apellido AS nombreAutor,
                p.fechaCreacion,
                p.fechaModificacion,
                p.usuarioModificacionId,
                (SELECT COUNT(*) 
                 FROM PublicacionesForo 
                 WHERE rootId = p.id AND estado = 1 AND id <> p.id) AS numeroRespuestas
            FROM PublicacionesForo p
            INNER JOIN usuario u ON p.usuarioId = u.id
            WHERE p.estado = 1 
              AND p.parentId IS NULL
            ORDER BY p.fechaCreacion DESC;

            IF NOT EXISTS (SELECT 1 FROM @Result)
                SET @Leyenda = 'No existen publicaciones en el foro';
            ELSE
                SET @Leyenda = 'Consulta exitosa';
        END
        
        -- Solo respuestas de un post
        ELSE IF (@iTransaccion = 'CONSULTAR_POST_COMPLETO')
        BEGIN
            SET @id = ISNULL(@iXml.value('(/PublicacionForo/Id)[1]', 'BIGINT'), 0);
            
            IF (@id <= 0)
            BEGIN
                SET @Respuesta = 'Error';
                SET @Leyenda = 'El identificador del post no es válido';
                GOTO FIN;
            END

            -- Validar que el post existe
            IF NOT EXISTS (SELECT 1 FROM PublicacionesForo WHERE id = @id AND estado = 1)
            BEGIN
                SET @Respuesta = 'Error';
                SET @Leyenda = 'Post no encontrado o fue eliminado';
                GOTO FIN;
            END

            -- Solo las respuestas 
            INSERT INTO @Result
            SELECT 
                r.id,
                NULL AS titulo, -- Las respuestas no tienen título
                r.contenido,
                r.urlImagen,
                r.usuarioId,
                r.parentId,
                r.rootId,
                u.nombre + ' ' + u.apellido AS nombreAutor,
                r.fechaCreacion,
                r.fechaModificacion,
                r.usuarioModificacionId,
                0 AS numeroRespuestas -- Las respuestas no calculan sub-respuestas
            FROM PublicacionesForo r
            INNER JOIN usuario u ON r.usuarioId = u.id
            WHERE r.rootId = @id 
              AND r.id <> @id 
              AND r.estado = 1
            ORDER BY r.fechaCreacion ASC;

            IF NOT EXISTS (SELECT 1 FROM @Result)
                SET @Leyenda = 'El post no tiene respuestas aún';
            ELSE
                SET @Leyenda = 'Respuestas consultadas exitosamente';
        END

        -- Publicaciones por usuario
        ELSE IF (@iTransaccion = 'CONSULTAR_POSTS_USUARIO')
        BEGIN
            SET @usuarioId = ISNULL(@iXml.value('(/PublicacionForo/UsuarioId)[1]', 'INT'), 0);

            IF (@usuarioId <= 0)
            BEGIN
                SET @Respuesta = 'Error';
                SET @Leyenda = 'El identificador del usuario no es válido';
                GOTO FIN;
            END

            INSERT INTO @Result
            SELECT 
                p.id,
                p.titulo,
                p.contenido,
                p.urlImagen,
                p.usuarioId,
                p.parentId,
                p.rootId,
                u.nombre + ' ' + u.apellido AS nombreAutor,
                p.fechaCreacion,
                p.fechaModificacion,
                p.usuarioModificacionId,
                (SELECT COUNT(*) 
                 FROM PublicacionesForo 
                 WHERE rootId = p.id 
                   AND id <> p.id
                   AND estado = 1) AS numeroRespuestas
            FROM PublicacionesForo p
            INNER JOIN usuario u ON p.usuarioId = u.id
            WHERE p.usuarioId = @usuarioId 
              AND p.estado = 1
            ORDER BY p.fechaCreacion DESC;

            IF NOT EXISTS (SELECT 1 FROM @Result)
                SET @Leyenda = 'El usuario no tiene publicaciones';
            ELSE
                SET @Leyenda = 'Consulta exitosa';
        END
        
        -- Búsqueda
        ELSE IF (@iTransaccion = 'BUSCAR_POSTS')
        BEGIN
            SET @textoBusqueda = LTRIM(RTRIM(ISNULL(@iXml.value('(/PublicacionForo/TextoBusqueda)[1]', 'NVARCHAR(200)'), '')));
            
            INSERT INTO @Result
            SELECT 
                p.id,
                p.titulo,
                p.contenido,
                p.urlImagen,
                p.usuarioId,
                p.parentId,
                p.rootId,
                u.nombre + ' ' + u.apellido AS nombreAutor,
                p.fechaCreacion,
                p.fechaModificacion,
                p.usuarioModificacionId,
                (SELECT COUNT(*) 
                 FROM PublicacionesForo 
                 WHERE rootId = p.id AND estado = 1 AND id <> p.id) AS numeroRespuestas
            FROM PublicacionesForo p
            INNER JOIN usuario u ON p.usuarioId = u.id
            WHERE p.estado = 1 
              AND p.parentId IS NULL
              AND (p.titulo LIKE '%' + @textoBusqueda + '%' 
                   OR p.contenido LIKE '%' + @textoBusqueda + '%')
            ORDER BY p.fechaCreacion DESC;

            IF NOT EXISTS (SELECT 1 FROM @Result)
                SET @Leyenda = 'No se encontraron resultados';
            ELSE
                SET @Leyenda = 'Búsqueda exitosa';
        END

        ELSE
        BEGIN
            SET @Respuesta = 'Error';
            SET @Leyenda = 'Transacción no válida';
        END

        FIN:
        -- Primero: Respuesta y Leyenda
        SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;

        -- Segundo: Tabla de datos de foros
        SELECT * FROM @Result;
        
    END TRY
    BEGIN CATCH
        SET @Respuesta = 'Error';
        SET @Leyenda = ERROR_MESSAGE();
        
        SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;
        
        -- tabla vacia
        SELECT TOP 0 * FROM @Result;
    END CATCH
END
GO

-- ===============================
-- SetPublicacionForo (SIN CAMBIOS)
-- ===============================
CREATE OR ALTER PROCEDURE [dbo].[SetPublicacionForo]
    @iTransaccion VARCHAR(50),
    @iXml XML = NULL
AS
BEGIN
    SET XACT_ABORT ON;
    
    DECLARE @Respuesta VARCHAR(10);
    DECLARE @Leyenda VARCHAR(200);
    
    DECLARE @id BIGINT, @parentId BIGINT, @rootId BIGINT;
    DECLARE @titulo NVARCHAR(200), @contenido NVARCHAR(MAX), @urlImagen VARCHAR(500);
    DECLARE @usuarioId INT;
    
    BEGIN TRY
        BEGIN TRANSACTION TRX_PUBLICACION_FORO;
        
        -- INSERTAR POST O RESPUESTA
        IF (@iTransaccion IN ('INSERTAR_POST', 'INSERTAR_RESPUESTA'))
        BEGIN
            SET @titulo    = LTRIM(RTRIM(ISNULL(@iXml.value('(/PublicacionForo/Titulo)[1]', 'NVARCHAR(200)'), '')));
            SET @contenido = LTRIM(RTRIM(ISNULL(@iXml.value('(/PublicacionForo/Contenido)[1]', 'NVARCHAR(MAX)'), '')));
            SET @urlImagen = LTRIM(RTRIM(ISNULL(@iXml.value('(/PublicacionForo/UrlImagen)[1]', 'VARCHAR(500)'), '')));
            SET @usuarioId = ISNULL(@iXml.value('(/PublicacionForo/UsuarioId)[1]', 'INT'), 0);
            SET @parentId  = @iXml.value('(/PublicacionForo/ParentId)[1]', 'BIGINT');

            IF (@contenido = '') SET @contenido = '(Sin contenido)';

            IF (@iTransaccion = 'INSERTAR_POST')
            BEGIN
                INSERT INTO PublicacionesForo (titulo, contenido, urlImagen, usuarioId, fechaCreacion, estado)
                VALUES (@titulo, @contenido, @urlImagen, @usuarioId, SYSUTCDATETIME(), 1);
                
                SET @id = SCOPE_IDENTITY();
                UPDATE PublicacionesForo SET rootId = @id WHERE id = @id;
                
                SET @Respuesta = 'Ok';
                SET @Leyenda = 'Post creado exitosamente';
            END
            ELSE -- INSERTAR_RESPUESTA
            BEGIN
                IF NOT EXISTS (SELECT 1 FROM PublicacionesForo WHERE id = @parentId AND estado = 1)
                BEGIN
                    SET @Respuesta = 'Error';
                    SET @Leyenda = 'El post padre no existe o fue eliminado';
                    GOTO FIN;
                END
                
                SELECT @rootId = ISNULL(rootId, id) FROM PublicacionesForo WHERE id = @parentId;
                
                INSERT INTO PublicacionesForo (parentId, rootId, contenido, urlImagen, usuarioId, fechaCreacion, estado)
                VALUES (@parentId, @rootId, @contenido, @urlImagen, @usuarioId, SYSUTCDATETIME(), 1);
                
                SET @Respuesta = 'Ok';
                SET @Leyenda = 'Respuesta enviada correctamente';
            END
        END
        
        -- ACTUALIZAR PUBLICACIÓN
        ELSE IF (@iTransaccion = 'ACTUALIZAR_PUBLICACION')
        BEGIN
            SET @id        = ISNULL(@iXml.value('(/PublicacionForo/Id)[1]', 'BIGINT'), 0);
            SET @titulo    = LTRIM(RTRIM(@iXml.value('(/PublicacionForo/Titulo)[1]', 'NVARCHAR(200)')));
            SET @contenido = LTRIM(RTRIM(ISNULL(@iXml.value('(/PublicacionForo/Contenido)[1]', 'NVARCHAR(MAX)'), '')));
            SET @urlImagen = LTRIM(RTRIM(@iXml.value('(/PublicacionForo/UrlImagen)[1]', 'VARCHAR(500)')));
            SET @usuarioId = ISNULL(@iXml.value('(/PublicacionForo/UsuarioModificacionId)[1]', 'INT'), 0);
            SET @usuarioId = ISNULL(@iXml.value('(/PublicacionForo/UsuarioModificacionId)[1]', 'INT'),
                             ISNULL(@iXml.value('(/PublicacionForo/UsuarioId)[1]', 'INT'), 0)
    );
            
            UPDATE PublicacionesForo
            SET titulo = ISNULL(@titulo, titulo),
                contenido = CASE WHEN @contenido = '' THEN contenido ELSE @contenido END,
                urlImagen = ISNULL(@urlImagen, urlImagen),
                fechaModificacion = SYSUTCDATETIME(),
                usuarioModificacionId = @usuarioId
            WHERE id = @id AND estado = 1;
            
            SET @Respuesta = 'Ok';
            SET @Leyenda = 'Publicación actualizada correctamente';
        END

        -- ELIMINACIÓN LÓGICA
        ELSE IF (@iTransaccion = 'ELIMINAR_PUBLICACION')
        BEGIN
            SET @id = ISNULL(@iXml.value('(/PublicacionForo/Id)[1]', 'BIGINT'), 0);
            SET @usuarioId = ISNULL(@iXml.value('(/PublicacionForo/UsuarioEliminacionId)[1]', 'INT'), 0);
            
            UPDATE PublicacionesForo
            SET estado = 0, 
                fechaEliminacion = SYSUTCDATETIME(), 
                usuarioEliminacionId = @usuarioId
            WHERE id = @id OR rootId = @id;
            
            SET @Respuesta = 'Ok';
            SET @Leyenda = 'Publicación eliminada correctamente';
        END

        ELSE
        BEGIN
            SET @Respuesta = 'Error';
            SET @Leyenda = 'Transacción no válida';
        END
        
        FIN:
        COMMIT TRANSACTION TRX_PUBLICACION_FORO;
        SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION TRX_PUBLICACION_FORO;

        SET @Respuesta = 'Error';
        SET @Leyenda = ERROR_MESSAGE();
        SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;
    END CATCH
END
GO