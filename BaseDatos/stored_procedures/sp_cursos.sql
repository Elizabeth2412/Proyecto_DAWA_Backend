USE AgroPetech;
GO
CREATE OR ALTER PROCEDURE GetCurso
    @iTransaccion VARCHAR(50),
    @iXML XML = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Respuesta VARCHAR(10) = 'Ok';
    DECLARE @Leyenda VARCHAR(200) = 'Ejecutado correctamente';
    DECLARE @ResultTable TABLE (
        id INT,
        titulo VARCHAR(200),
        descripcion VARCHAR(1000),
        nivel VARCHAR(50),
        duracion INT,
        instructor VARCHAR(100),
        fechaCreacion DATETIME,
        fechaActualizacion DATETIME,
        transaccion VARCHAR(50)
    );

    BEGIN TRY
        IF (@iTransaccion = 'LISTAR_CURSOS')
        BEGIN
            -- Insertar todos los cursos
            INSERT INTO @ResultTable
            SELECT 
                id,
                titulo,
                descripcion,
                nivel,
                duracion,
                instructor,
                fechaCreacion,
                fechaActualizacion,
                @iTransaccion
            FROM Curso
            ORDER BY fechaCreacion DESC;
            
            SET @Leyenda = 'Consulta exitosa. Cursos encontrados: ' + CAST(@@ROWCOUNT AS VARCHAR);
        END
        ELSE
        BEGIN
            SET @Respuesta = 'Error';
            SET @Leyenda = 'Transacción no válida: ' + @iTransaccion;
        END

        -- Devolver resultados en la estructura esperada
        SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;
        
        IF @Respuesta = 'Ok'
            SELECT * FROM @ResultTable;
        ELSE
            SELECT TOP 0 * FROM @ResultTable;

    END TRY
    BEGIN CATCH
        SET @Respuesta = 'Error';
        SET @Leyenda = 'Error en la consulta: ' + ERROR_MESSAGE();
        
        SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;
        SELECT TOP 0 * FROM @ResultTable;
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE SetCurso
    @iTransaccion VARCHAR(50),
    @iXML XML
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Respuesta VARCHAR(10);
    DECLARE @Leyenda  VARCHAR(200);

    BEGIN TRY
        BEGIN TRANSACTION;

        -- INSERTAR
        IF (@iTransaccion = 'INSERTAR_CURSO')
        BEGIN
            INSERT INTO Curso (
                titulo,
                descripcion,
                nivel,
                duracion,
                instructor
            )
            VALUES (
                @iXML.value('(/Curso/Titulo)[1]', 'VARCHAR(200)'),
                @iXML.value('(/Curso/Descripcion)[1]', 'VARCHAR(1000)'),
                @iXML.value('(/Curso/Nivel)[1]', 'VARCHAR(50)'),
                @iXML.value('(/Curso/Duracion)[1]', 'INT'),
                @iXML.value('(/Curso/Instructor)[1]', 'VARCHAR(100)')
            );

            SET @Respuesta = 'Ok';
            SET @Leyenda  = 'Curso creado correctamente';
        END

        -- ACTUALIZAR
        ELSE IF (@iTransaccion = 'ACTUALIZAR_CURSO')
        BEGIN
            UPDATE Curso
            SET
                titulo = @iXML.value('(/Curso/Titulo)[1]', 'VARCHAR(200)'),
                descripcion = @iXML.value('(/Curso/Descripcion)[1]', 'VARCHAR(1000)'),
                nivel = @iXML.value('(/Curso/Nivel)[1]', 'VARCHAR(50)'),
                duracion = @iXML.value('(/Curso/Duracion)[1]', 'INT'),
                fechaActualizacion = GETDATE()
            WHERE id = @iXML.value('(/Curso/Id)[1]', 'INT');

            SET @Respuesta = 'Ok';
            SET @Leyenda  = 'Curso actualizado correctamente';
        END

        -- ELIMINAR (FÍSICO)
        ELSE IF (@iTransaccion = 'ELIMINAR_CURSO')
        BEGIN
            DELETE FROM Curso
            WHERE id = @iXML.value('(/Curso/Id)[1]', 'INT');

            SET @Respuesta = 'Ok';
            SET @Leyenda  = 'Curso eliminado correctamente';
        END

        COMMIT;
        SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;

    END TRY
    BEGIN CATCH
        ROLLBACK;
        SELECT 'Error' AS Respuesta, ERROR_MESSAGE() AS Leyenda;
    END CATCH
END;
GO
    INSERT INTO Curso (titulo, descripcion, nivel, duracion, instructor)
    VALUES 
    ('Introducción a la agricultura', 'Curso básico para principiantes', 'Principiante', 10, 'leslie@gmail.com');
    GO 
    SELECT * FROM Curso;
    SELECT * FROM Archivo;
    
    SELECT * FROM CursoArchivo;