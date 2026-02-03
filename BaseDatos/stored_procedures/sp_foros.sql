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
    
    DECLARE @respuesta VARCHAR(50) = 'ok';
    DECLARE @leyenda VARCHAR(200) = 'Consulta exitosa';
    DECLARE @id BIGINT;
    DECLARE @usuarioId INT;
    DECLARE @textoBusqueda NVARCHAR(200);
    
    BEGIN TRY
        
        -- Posts raíz
        IF (@iTransaccion = 'CONSULTAR_POSTS')
        BEGIN
            SELECT 
                p.Id,
                p.Titulo,
                p.Contenido,
                p.UrlImagen,
                p.UsuarioId,
                u.Nombre + ' ' + u.Apellido AS NombreAutor,
                p.FechaCreacion,
                p.FechaModificacion,
                (SELECT COUNT(*) 
                 FROM PublicacionesForo 
                 WHERE RootId = p.Id AND Estado = 1) AS NumeroRespuestas
            FROM PublicacionesForo p
            INNER JOIN Usuarios u ON p.UsuarioId = u.Id
            WHERE p.Estado = 1 
              AND p.ParentId IS NULL
            ORDER BY p.FechaCreacion DESC;
        END
        
        -- Post con respuestas
        IF (@iTransaccion = 'CONSULTAR_POST_COMPLETO')
        BEGIN
            SET @id = @iXml.value('(/Post/Id)[1]', 'BIGINT');
            
            SELECT 
                p.Id,
                p.Titulo,
                p.Contenido,
                p.UrlImagen,
                p.UsuarioId,
                u.Nombre + ' ' + u.Apellido AS NombreAutor,
                p.FechaCreacion,
                p.FechaModificacion
            FROM PublicacionesForo p
            INNER JOIN Usuarios u ON p.UsuarioId = u.Id
            WHERE p.Id = @id 
              AND p.Estado = 1;
            
            SELECT 
                r.Id,
                r.ParentId,
                r.Contenido,
                r.UrlImagen,
                r.UsuarioId,
                u.Nombre + ' ' + u.Apellido AS NombreAutor,
                r.FechaCreacion,
                r.FechaModificacion
            FROM PublicacionesForo r
            INNER JOIN Usuarios u ON r.UsuarioId = u.Id
            WHERE r.RootId = @id 
              AND r.Estado = 1
            ORDER BY r.FechaCreacion ASC;
        END
        
        -- Posts por usuario
        IF (@iTransaccion = 'CONSULTAR_POSTS_USUARIO')
        BEGIN
            SET @usuarioId = @iXml.value('(/Post/UsuarioId)[1]', 'INT');
            
            SELECT 
                p.Id,
                p.Titulo,
                p.Contenido,
                p.UrlImagen,
                p.ParentId,
                p.RootId,
                p.FechaCreacion,
                (SELECT COUNT(*) 
                 FROM PublicacionesForo 
                 WHERE RootId = p.Id AND Estado = 1) AS NumeroRespuestas
            FROM PublicacionesForo p
            WHERE p.UsuarioId = @usuarioId 
              AND p.Estado = 1
            ORDER BY p.FechaCreacion DESC;
        END
        
        -- Búsqueda (Full-Text / fallback)
        IF (@iTransaccion = 'BUSCAR_POSTS')
        BEGIN
            SET @textoBusqueda = @iXml.value('(/Post/TextoBusqueda)[1]', 'NVARCHAR(200)');
            
            SELECT 
                p.Id,
                p.Titulo,
                p.Contenido,
                p.UrlImagen,
                p.UsuarioId,
                u.Nombre + ' ' + u.Apellido AS NombreAutor,
                p.FechaCreacion
            FROM PublicacionesForo p
            INNER JOIN Usuarios u ON p.UsuarioId = u.Id
            WHERE p.Estado = 1
              AND p.ParentId IS NULL
              AND (
                    CONTAINS((Titulo, Contenido), @textoBusqueda)
                 OR p.Titulo LIKE '%' + @textoBusqueda + '%'
                 OR p.Contenido LIKE '%' + @textoBusqueda + '%'
              )
            ORDER BY p.FechaCreacion DESC;
        END
        
        -- Posts recientes
        IF (@iTransaccion = 'CONSULTAR_POSTS_RECIENTES')
        BEGIN
            SELECT TOP 10
                p.Id,
                p.Titulo,
                p.Contenido,
                p.UrlImagen,
                p.UsuarioId,
                u.Nombre + ' ' + u.Apellido AS NombreAutor,
                p.FechaCreacion,
                (SELECT COUNT(*) 
                 FROM PublicacionesForo 
                 WHERE RootId = p.Id AND Estado = 1) AS NumeroRespuestas
            FROM PublicacionesForo p
            INNER JOIN Usuarios u ON p.UsuarioId = u.Id
            WHERE p.Estado = 1 
              AND p.ParentId IS NULL
            ORDER BY p.FechaCreacion DESC;
        END
        
    END TRY
    BEGIN CATCH
        SET @respuesta = 'Error';
        SET @leyenda = ERROR_MESSAGE();
    END CATCH
    
    SELECT @respuesta AS respuesta, @leyenda AS leyenda;
END
GO

-- ===============================
-- SetPublicacionForo
-- ===============================
CREATE OR ALTER PROCEDURE [dbo].[SetPublicacionForo]
    @iTransaccion VARCHAR(50),
    @iXml XML = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @respuesta VARCHAR(50) = 'ok';
    DECLARE @leyenda VARCHAR(200);
    DECLARE @id BIGINT;
    DECLARE @parentId BIGINT;
    DECLARE @rootId BIGINT;
    DECLARE @titulo NVARCHAR(200);
    DECLARE @contenido NVARCHAR(MAX);
    DECLARE @urlImagen VARCHAR(500);
    DECLARE @usuarioId INT;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Insertar post raíz
        IF (@iTransaccion = 'INSERTAR_POST')
        BEGIN
            SET @titulo = @iXml.value('(/Post/Titulo)[1]', 'NVARCHAR(200)');
            SET @contenido = @iXml.value('(/Post/Contenido)[1]', 'NVARCHAR(MAX)');
            SET @urlImagen = @iXml.value('(/Post/UrlImagen)[1]', 'VARCHAR(500)');
            SET @usuarioId = @iXml.value('(/Post/UsuarioId)[1]', 'INT');
            
            INSERT INTO PublicacionesForo
                (Titulo, Contenido, UrlImagen, UsuarioId, FechaCreacion, Estado)
            VALUES
                (@titulo, @contenido, @urlImagen, @usuarioId, SYSUTCDATETIME(), 1);
            
            SET @id = SCOPE_IDENTITY();
            UPDATE PublicacionesForo SET RootId = @id WHERE Id = @id;
            
            SET @leyenda = 'Post creado: ' + CAST(@id AS VARCHAR(20));
        END
        
        -- Insertar respuesta
        IF (@iTransaccion = 'INSERTAR_RESPUESTA')
        BEGIN
            SET @parentId = @iXml.value('(/Respuesta/ParentId)[1]', 'BIGINT');
            SET @contenido = @iXml.value('(/Respuesta/Contenido)[1]', 'NVARCHAR(MAX)');
            SET @urlImagen = @iXml.value('(/Respuesta/UrlImagen)[1]', 'VARCHAR(500)');
            SET @usuarioId = @iXml.value('(/Respuesta/UsuarioId)[1]', 'INT');
            
            SELECT @rootId = ISNULL(RootId, Id)
            FROM PublicacionesForo 
            WHERE Id = @parentId;
            
            INSERT INTO PublicacionesForo
                (ParentId, RootId, Contenido, UrlImagen, UsuarioId, FechaCreacion, Estado)
            VALUES
                (@parentId, @rootId, @contenido, @urlImagen, @usuarioId, SYSUTCDATETIME(), 1);
            
            SET @id = SCOPE_IDENTITY();
            SET @leyenda = 'Respuesta creada: ' + CAST(@id AS VARCHAR(20));
        END
        
        -- Actualizar publicación
        IF (@iTransaccion = 'ACTUALIZAR_PUBLICACION')
        BEGIN
            SET @id = @iXml.value('(/Post/Id)[1]', 'BIGINT');
            SET @titulo = @iXml.value('(/Post/Titulo)[1]', 'NVARCHAR(200)');
            SET @contenido = @iXml.value('(/Post/Contenido)[1]', 'NVARCHAR(MAX)');
            SET @urlImagen = @iXml.value('(/Post/UrlImagen)[1]', 'VARCHAR(500)');
            SET @usuarioId = @iXml.value('(/Post/UsuarioModificacionId)[1]', 'INT');
            
            UPDATE PublicacionesForo
            SET 
                Titulo = ISNULL(@titulo, Titulo),
                Contenido = @contenido,
                UrlImagen = @urlImagen,
                FechaModificacion = SYSUTCDATETIME(),
                UsuarioModificacionId = @usuarioId
            WHERE Id = @id 
              AND Estado = 1;
            
            SET @leyenda = 'Publicación actualizada: ' + CAST(@id AS VARCHAR(20));
        END
        
        -- Eliminación lógica
        IF (@iTransaccion = 'ELIMINAR_PUBLICACION')
        BEGIN
            SET @id = @iXml.value('(/Post/Id)[1]', 'BIGINT');
            SET @usuarioId = @iXml.value('(/Post/UsuarioEliminacionId)[1]', 'INT');
            
            UPDATE PublicacionesForo
            SET 
                Estado = 0,
                FechaEliminacion = SYSUTCDATETIME(),
                UsuarioEliminacionId = @usuarioId
            WHERE Id = @id;
            
            IF EXISTS (
                SELECT 1 
                FROM PublicacionesForo 
                WHERE Id = @id AND ParentId IS NULL
            )
            BEGIN
                UPDATE PublicacionesForo
                SET 
                    Estado = 0,
                    FechaEliminacion = SYSUTCDATETIME(),
                    UsuarioEliminacionId = @usuarioId
                WHERE RootId = @id;
            END
            
            SET @leyenda = 'Publicación eliminada: ' + CAST(@id AS VARCHAR(20));
        END
        
        COMMIT TRANSACTION;
        
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @respuesta = 'Error';
        SET @leyenda = ERROR_MESSAGE();
    END CATCH
    
    SELECT @respuesta AS respuesta, @leyenda AS leyenda;
END
GO

PRINT 'Stored Procedures de Foros creados';
GO

