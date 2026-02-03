USE AgroPetech;
GO
CREATE OR ALTER PROCEDURE GetCurso
    @iTransaccion VARCHAR(50),
    @iXML XML = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Respuesta VARCHAR(10) = 'Ok';
    DECLARE @Leyenda   VARCHAR(200) = 'Ejecutado correctamente';

    BEGIN TRY

        -- LISTAR CURSOS
        IF (@iTransaccion = 'LISTAR_CURSOS')
        BEGIN
            SET @Leyenda = 'Consulta exitosa';

            -- 1️⃣ RESPUESTA (SIEMPRE PRIMERO)
            SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;

            -- 2️⃣ DATOS
            SELECT 
                id,
                titulo,
                descripcion,
                nivel,
                duracion,
                instructor,
                fechaCreacion,
                fechaActualizacion,
                @iTransaccion AS transaccion
            FROM Curso
            ORDER BY fechaCreacion DESC;
        END

        -- LISTAR CURSOS CON ARCHIVOS
        ELSE IF (@iTransaccion = 'LISTAR_CURSOS_CON_ARCHIVOS')
        BEGIN
            SET @Leyenda = 'Consulta con archivos';

            -- 1️⃣ RESPUESTA
            SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;

            -- 2️⃣ CURSOS
            SELECT 
                c.id,
                c.titulo,
                c.descripcion,
                c.nivel,
                c.duracion,
                c.instructor,
                c.fechaCreacion,
                c.fechaActualizacion,
                @iTransaccion AS transaccion
            FROM Curso c
            ORDER BY c.fechaCreacion DESC;

            -- 3️⃣ ARCHIVOS
            SELECT 
                a.id AS archivoId,
                a.nombre,
                a.tipo,
                a.tamano,
                a.descripcion,
                a.usuario,
                a.estado,
                a.fechaSubida,
                ca.cursoId
            FROM Archivo a
            INNER JOIN CursoArchivo ca ON a.id = ca.archivoId
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
        SELECT 'Error' AS Respuesta, ERROR_MESSAGE() AS Leyenda;
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
    SELECT * FROM Usuario;