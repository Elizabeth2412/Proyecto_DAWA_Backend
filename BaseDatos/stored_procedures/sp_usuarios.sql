USE AgroPetech;
GO
-- Procedimientos  para Usuario
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
            
            SET @Leyenda = 'Validaci�n Exitosa';
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
            
            SET @Leyenda = 'B�squeda Exitosa';
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
            SET @Leyenda = 'Contrase�a actualizada para: ' + @emailC;
        END

        ELSE
        BEGIN
            SET @Respuesta = 'Error';
            SET @Leyenda = 'Transacci�n no v�lida: ' + @iTransaccion;
        END

        COMMIT TRANSACTION TRX_USUARIO;
        
        -- Devolver resultado
        SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION TRX_USUARIO;

        SET @Respuesta = 'Error';
        SET @Leyenda = 'Error en la transacci�n ' + @iTransaccion + ': ' + ERROR_MESSAGE();
        
        SELECT @Respuesta AS Respuesta, @Leyenda AS Leyenda;
    END CATCH
END;
GO
CREATE NONCLUSTERED INDEX IX_Usuario_Email_Password
ON Usuario (email, password)
INCLUDE (id, tipo, nombre, apellido, edad);
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_Usuario_Email
ON Usuario (email);
GO