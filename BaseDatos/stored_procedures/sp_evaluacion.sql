USE Agropetech;

EXEC GetEvaluacion @iTransaccion = 'CONSULTAR_EVALUACION'

SELECT * FROM Evaluacion

---------------------------------
   -- <<GET EVALUACION>> --
---------------------------------

GO
CREATE OR ALTER PROCEDURE GetEvaluacion
    @iTransaccion VARCHAR(50),
    @iXML XML = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Respuesta VARCHAR(10) = 'Ok';
    DECLARE @Leyenda VARCHAR(200) = 'Ejecutado correctamente';

    DECLARE @Result TABLE (
        id INT,
        cursoId INT,
        titulo VARCHAR(200),
        modulo VARCHAR(100),
        totalPreguntas INT,
        duracion VARCHAR(50),
        estado VARCHAR(20)
    );

    BEGIN TRY

        IF (@iTransaccion = 'CONSULTAR_EVALUACION')
        BEGIN
            IF EXISTS (SELECT 1 FROM Evaluacion)
            BEGIN
                INSERT INTO @Result
                SELECT 
                    id, 
                    cursoId, 
                    titulo, 
                    modulo, 
                    totalPreguntas, 
                    duracion, 
                    estado
                FROM Evaluacion;

                SET @Leyenda = 'Consulta exitosa';
            END
            ELSE
            BEGIN
                SET @Respuesta = 'Error';
                SET @Leyenda = 'No existen evaluaciones registradas';
            END
        END

        -- CONSULTA DE EVALUACIONES POR CURSO
        ELSE IF (@iTransaccion = 'EVALUACION_POR_CURSO')
        BEGIN
            DECLARE @cursoId INT =
                ISNULL(@iXML.value('(/Evaluacion/CursoId)[1]', 'INT'), 0);

            IF @cursoId <= 0
            BEGIN
                SET @Respuesta = 'Error';
                SET @Leyenda = 'El identificador del curso no es válido';
            END

            -- Validar si existe el curso
            ELSE IF NOT EXISTS (SELECT 1 FROM Curso WHERE id = @cursoId)
            BEGIN
                SET @Respuesta = 'Error';
                SET @Leyenda = 'No existe un curso con la identificación proporcionada';
            END

            -- Validar si el curso tiene evaluaciones asociadas
            ELSE IF NOT EXISTS (SELECT 1 FROM Evaluacion WHERE cursoId = @cursoId)
            BEGIN
                SET @Respuesta = 'Error';
                SET @Leyenda = 'El curso no tiene evaluaciones asociadas';
            END
            ELSE
            BEGIN
                INSERT INTO @Result
                SELECT 
                    id, 
                    cursoId, 
                    titulo, 
                    modulo, 
                    totalPreguntas, 
                    duracion, 
                    estado
                FROM Evaluacion
                WHERE cursoId = @cursoId;

                SET @Leyenda = 'Consulta por curso exitosa';
            END
        END


        -- CONSULTA DE EVALUACION POR NOMBRE
        ELSE IF (@iTransaccion = 'BUSCAR_EVALUACION')
        BEGIN
            DECLARE @titulo VARCHAR(200) = LTRIM(RTRIM(ISNULL(@iXML.value('(/Evaluacion/Titulo)[1]', 'VARCHAR(200)'), '')));

            IF @titulo = ''
            BEGIN
                SET @Respuesta = 'Error';
                SET @Leyenda = 'El nombre de la evaluación es obligatorio';
            END
            -- Validar existencia
            ELSE IF NOT EXISTS (SELECT 1 FROM Evaluacion WHERE titulo LIKE '%' + @titulo + '%')
            BEGIN
                SET @Respuesta = 'Error';
                SET @Leyenda = 'No existe una evaluación con el nombre proporcionado';
            END
            ELSE
            BEGIN
                INSERT INTO @Result
                SELECT 
                    id, 
                    cursoId, 
                    titulo, 
                    modulo, 
                    totalPreguntas, 
                    duracion, 
                    estado
                FROM Evaluacion
                WHERE titulo LIKE '%' + @titulo + '%';

                SET @Leyenda = 'Búsqueda por nombre exitosa';
            END
        END


        ELSE
        BEGIN
            SET @Respuesta = 'Error';
            SET @Leyenda = 'Transacción no válida';
        END

        SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;
        SELECT * FROM @Result;

    END TRY
    BEGIN CATCH
        SET @Respuesta = 'Error';
        SET @Leyenda = ERROR_MESSAGE();

        SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;
        SELECT TOP 0 * FROM @Result;
    END CATCH
END;
GO


---------------------------------
   -- <<SET EVALUACION>> --
---------------------------------

GO
CREATE OR ALTER PROCEDURE SetEvaluacion
    @iTransaccion VARCHAR(50),
    @iXML XML = NULL
AS
BEGIN
    SET XACT_ABORT ON;

    DECLARE @Respuesta VARCHAR(10);
    DECLARE @Leyenda VARCHAR(200);

    BEGIN TRY
        BEGIN TRANSACTION TRX_EVALUACION;

        -- INSERTAR
        IF (@iTransaccion = 'INSERTAR_EVALUACION')
        BEGIN
            INSERT INTO Evaluacion (
                cursoId,
                titulo,
                modulo,
                totalPreguntas,
                duracion,
                estado,
                usuarioCreacionId
            )
            VALUES (
                NULLIF(@iXML.value('(/Evaluacion/CursoId)[1]', 'INT'), 0),
                @iXML.value('(/Evaluacion/Titulo)[1]', 'VARCHAR(200)'),
                @iXML.value('(/Evaluacion/Modulo)[1]', 'VARCHAR(100)'),
                @iXML.value('(/Evaluacion/TotalPreguntas)[1]', 'INT'),
                @iXML.value('(/Evaluacion/Duracion)[1]', 'VARCHAR(50)'),
                'Activa',
                @iXML.value('(/Evaluacion/UsuarioId)[1]', 'INT')
            );

            SET @Respuesta = 'Ok';
            SET @Leyenda = 'Evaluación creada correctamente';
        END

        -- ACTUALIZAR
        ELSE IF (@iTransaccion = 'ACTUALIZAR_EVALUACION')
        BEGIN
            UPDATE Evaluacion SET 
                cursoId = NULLIF(@iXML.value('(/Evaluacion/CursoId)[1]', 'INT'), 0),
                titulo = @iXML.value('(/Evaluacion/Titulo)[1]', 'VARCHAR(200)'),
                modulo = @iXML.value('(/Evaluacion/Modulo)[1]', 'VARCHAR(100)'),
                duracion = @iXML.value('(/Evaluacion/Duracion)[1]', 'VARCHAR(50)'),
                estado = @iXML.value('(/Evaluacion/Estado)[1]', 'VARCHAR(20)'),
                fechaModificacion = GETDATE(),
                usuarioModificacionId = @iXML.value('(/Evaluacion/UsuarioId)[1]', 'INT')
            WHERE id = @iXML.value('(/Evaluacion/Id)[1]', 'INT');

            SET @Respuesta = 'Ok';
            SET @Leyenda = 'Evaluación actualizada correctamente';
        END

        -- ELIMINACIÓN LÓGICA
        ELSE IF (@iTransaccion = 'ELIMINAR_EVALUACION')
        BEGIN
            UPDATE Evaluacion
            SET estado = 'Inactiva',
                fechaEliminacion = GETDATE(),
                usuarioEliminacionId = @iXML.value('(/Evaluacion/UsuarioId)[1]', 'INT')
            WHERE id = @iXML.value('(/Evaluacion/Id)[1]', 'INT');

            SET @Respuesta = 'Ok';
            SET @Leyenda = 'Evaluación eliminada correctamente';
        END

        ELSE
        BEGIN
            SET @Respuesta = 'Error';
            SET @Leyenda = 'Transacción no válida';
        END

        COMMIT TRANSACTION TRX_EVALUACION;
        SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION TRX_EVALUACION;

        SET @Respuesta = 'Error';
        SET @Leyenda = ERROR_MESSAGE();

        SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;
    END CATCH
END;
GO



---------------------------------
   -- <<GET PREGUNTA>> --
---------------------------------

GO
CREATE OR ALTER PROCEDURE GetPregunta
    @iTransaccion VARCHAR(50),
    @iXML XML = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Respuesta VARCHAR(10) = 'Ok';
    DECLARE @Leyenda VARCHAR(200) = 'Ejecutado correctamente';

    DECLARE @Result TABLE (
        id INT,
        evaluacionId INT,
        texto VARCHAR(500),
        estado VARCHAR(20)
    );

    BEGIN TRY
           --CONSULTAR TODAS LAS PREGUNTAS
        IF (@iTransaccion = 'CONSULTAR_PREGUNTAS')
        BEGIN
            INSERT INTO @Result
            SELECT id, evaluacionId, texto, estado
            FROM Pregunta


            IF NOT EXISTS (SELECT 1 FROM @Result)
                SET @Leyenda = 'No existen preguntas registradas';
            ELSE
                SET @Leyenda = 'Consulta exitosa';
        END

        --PREGUNTAS POR EVALUACIÓN
        ELSE IF (@iTransaccion = 'PREGUNTAS_POR_EVALUACION')
        BEGIN
            DECLARE @evaluacionId INT = ISNULL(@iXML.value('(/Pregunta/EvaluacionId)[1]', 'INT'), 0);

            IF (@evaluacionId <= 0)
            BEGIN
                SET @Respuesta = 'Error';
                SET @Leyenda = 'EvaluacionId inválido';
            END

            INSERT INTO @Result
            SELECT id, evaluacionId, texto, estado
            FROM Pregunta
            WHERE evaluacionId = @evaluacionId

            IF NOT EXISTS (SELECT 1 FROM @Result)
                SET @Leyenda = 'No existen preguntas para esta evaluación';
            ELSE
                SET @Leyenda = 'Consulta exitosa';
        END

        --BUSCAR POR ID
        ELSE IF (@iTransaccion = 'BUSCAR_PREGUNTA')
        BEGIN

            DECLARE @id INT = ISNULL(@iXML.value('(/Pregunta/Id)[1]', 'INT'), 0);

            IF (@id <= 0)
            BEGIN
                SET @Respuesta = 'Error';
                SET @Leyenda = 'Id de pregunta inválido';
                GOTO FIN;
            END

            INSERT INTO @Result
            SELECT id, evaluacionId, texto, estado
            FROM Pregunta
            WHERE id = @id
              AND estado <> 'Inactiva';

            IF NOT EXISTS (SELECT 1 FROM @Result)
                SET @Leyenda = 'Pregunta no encontrada';
            ELSE
                SET @Leyenda = 'Búsqueda exitosa';
        END
        ELSE
        BEGIN
            SET @Respuesta = 'Error';
            SET @Leyenda = 'Transacción no válida';
        END

        FIN:
        SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;
        SELECT * FROM @Result;

    END TRY
    BEGIN CATCH
        SET @Respuesta = 'Error';
        SET @Leyenda = ERROR_MESSAGE();

        SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;
        SELECT TOP 0 * FROM @Result;
    END CATCH
END;
GO


---------------------------------
   -- <<SET PREGUNTA>> --
---------------------------------

GO
CREATE OR ALTER PROCEDURE SetPregunta
    @iTransaccion VARCHAR(50),
    @iXML XML = NULL
AS
BEGIN
    SET XACT_ABORT ON;

    DECLARE @Respuesta VARCHAR(10);
    DECLARE @Leyenda VARCHAR(200);

    BEGIN TRY
        BEGIN TRANSACTION TRX_PREGUNTA;

        -- INSERTAR PREGUNTA
        IF (@iTransaccion = 'INSERTAR_PREGUNTA')
        BEGIN
            INSERT INTO Pregunta (
                evaluacionId,
                texto,
                estado,
                usuarioCreacionId
            )
            VALUES (
                @iXML.value('(/Pregunta/EvaluacionId)[1]', 'INT'),
                @iXML.value('(/Pregunta/Texto)[1]', 'VARCHAR(500)'),
                'Activa',
                @iXML.value('(/Pregunta/UsuarioId)[1]', 'INT')
            );

            SET @Respuesta = 'Ok';
            SET @Leyenda = 'Pregunta creada correctamente';
        END

        -- ACTUALIZAR PREGUNTA
        ELSE IF (@iTransaccion = 'ACTUALIZAR_PREGUNTA')
        BEGIN
            UPDATE Pregunta
            SET texto = @iXML.value('(/Pregunta/Texto)[1]', 'VARCHAR(500)'),
                fechaModificacion = GETDATE(),
                usuarioModificacionId = @iXML.value('(/Pregunta/UsuarioId)[1]', 'INT')
            WHERE id = @iXML.value('(/Pregunta/Id)[1]', 'INT');

            SET @Respuesta = 'Ok';
            SET @Leyenda = 'Pregunta actualizada correctamente';
        END


        -- ELIMINAR PREGUNTA (LÓGICO)
        ELSE IF (@iTransaccion = 'ELIMINAR_PREGUNTA')
        BEGIN
            UPDATE Pregunta
            SET estado = 'Inactiva',
                fechaEliminacion = GETDATE(),
                usuarioEliminacionId = @iXML.value('(/Pregunta/UsuarioId)[1]', 'INT')
            WHERE id = @iXML.value('(/Pregunta/Id)[1]', 'INT');

            SET @Respuesta = 'Ok';
            SET @Leyenda = 'Pregunta eliminada correctamente';
        END

        ELSE
        BEGIN
            SET @Respuesta = 'Error';
            SET @Leyenda = 'Transacción no válida';
        END

        COMMIT TRANSACTION TRX_PREGUNTA;
        SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION TRX_PREGUNTA;

        SET @Respuesta = 'Error';
        SET @Leyenda = ERROR_MESSAGE();

        SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;
    END CATCH
END;
GO



---------------------------------
   -- <<GET OPCIÓN>> --
---------------------------------

GO
CREATE OR ALTER PROCEDURE GetOpcion
    @iTransaccion VARCHAR(50),
    @iXML XML = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Respuesta VARCHAR(10) = 'Ok';
    DECLARE @Leyenda VARCHAR(200) = 'Ejecutado correctamente';

    DECLARE @Result TABLE (
        id INT,
        preguntaId INT,
        texto VARCHAR(200),
        esCorrecta BIT,
        estado VARCHAR(20)
    );

    BEGIN TRY
        --CONSULTAR TODAS LAS OPCIONES EXISTENTES
        IF (@iTransaccion = 'CONSULTAR_OPCIONES')
        BEGIN
            INSERT INTO @Result
            SELECT 
                id, 
                preguntaId, 
                texto, 
                esCorrecta, 
                estado
            FROM Opcion


            IF NOT EXISTS (SELECT 1 FROM @Result)
                SET @Leyenda = 'No existen opciones registradas';
            ELSE
                SET @Leyenda = 'Consulta exitosa';
        END
        -- OPCIONES POR PREGUNTA
        ELSE IF (@iTransaccion = 'OPCIONES_POR_PREGUNTA')
        BEGIN
            DECLARE @preguntaId INT =
                ISNULL(@iXML.value('(/Opcion/PreguntaId)[1]', 'INT'), 0);

            INSERT INTO @Result
            SELECT id, preguntaId, texto, esCorrecta, estado
            FROM Opcion
            WHERE preguntaId = @preguntaId

            SET @Leyenda = 'Consulta exitosa';
        END

        ELSE
        BEGIN
            SET @Respuesta = 'Error';
            SET @Leyenda = 'Transacción no válida';
        END

        SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;
        SELECT * FROM @Result;

    END TRY
    BEGIN CATCH
        SET @Respuesta = 'Error';
        SET @Leyenda = ERROR_MESSAGE();

        SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;
        SELECT TOP 0 * FROM @Result;
    END CATCH
END;
GO



---------------------------------
   -- <<SET OPCIÓN>> --
---------------------------------

GO
CREATE OR ALTER PROCEDURE SetOpcion
    @iTransaccion VARCHAR(50),
    @iXML XML = NULL
AS
BEGIN
    SET XACT_ABORT ON;

    DECLARE @Respuesta VARCHAR(10);
    DECLARE @Leyenda VARCHAR(200);

    BEGIN TRY
        BEGIN TRANSACTION TRX_OPCION;

        -- INSERTAR OPCIÓN
        IF (@iTransaccion = 'INSERTAR_OPCION')
        BEGIN
            INSERT INTO Opcion (
                preguntaId,
                texto,
                esCorrecta,
                estado,
                usuarioCreacionId
            )
            VALUES (
                @iXML.value('(/Opcion/PreguntaId)[1]', 'INT'),
                @iXML.value('(/Opcion/Texto)[1]', 'VARCHAR(200)'),
                @iXML.value('(/Opcion/EsCorrecta)[1]', 'BIT'),
                'Activa',
                @iXML.value('(/Opcion/UsuarioId)[1]', 'INT')
            );

            SET @Respuesta = 'Ok';
            SET @Leyenda = 'Opción creada correctamente';
        END

        -- ACTUALIZAR OPCIÓN
        ELSE IF (@iTransaccion = 'ACTUALIZAR_OPCION')
        BEGIN
            UPDATE Opcion
            SET texto = @iXML.value('(/Opcion/Texto)[1]', 'VARCHAR(200)'),
                esCorrecta = @iXML.value('(/Opcion/EsCorrecta)[1]', 'BIT'),
                fechaModificacion = GETDATE(),
                usuarioModificacionId = @iXML.value('(/Opcion/UsuarioId)[1]', 'INT')
            WHERE id = @iXML.value('(/Opcion/Id)[1]', 'INT');

            SET @Respuesta = 'Ok';
            SET @Leyenda = 'Opción actualizada correctamente';
        END

        -- ELIMINAR OPCIÓN (LÓGICO)
        ELSE IF (@iTransaccion = 'ELIMINAR_OPCION')
        BEGIN
            UPDATE Opcion
            SET estado = 'Inactiva',
                fechaEliminacion = GETDATE(),
                usuarioEliminacionId = @iXML.value('(/Opcion/UsuarioId)[1]', 'INT')
            WHERE id = @iXML.value('(/Opcion/Id)[1]', 'INT');

            SET @Respuesta = 'Ok';
            SET @Leyenda = 'Opción eliminada correctamente';
        END

        ELSE
        BEGIN
            SET @Respuesta = 'Error';
            SET @Leyenda = 'Transacción no válida';
        END

        COMMIT TRANSACTION TRX_OPCION;
        SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION TRX_OPCION;

        SET @Respuesta = 'Error';
        SET @Leyenda = ERROR_MESSAGE();

        SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;
    END CATCH
END;
GO
