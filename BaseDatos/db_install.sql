/* ============================================
   Proyecto: Agropetech
   Script: db_install.sql
   Motor: SQL Server (Modo SQLCMD)
   ============================================ */

SET NOCOUNT ON;
GO
:setvar DatabaseName "Agropetech"

-- =====================================================
-- CONFIGURACIÓN DE RUTA BASE
-- =====================================================
-- Si ejecutas desde CMD (sqlcmd):
--   → deja BASE_PATH vacío y solo ejecuta en el CMD, desde la ruta donde esta el archivo
--   → sqlcmd -S . -E -i db_install.sql
-- Si ejecutas desde SSMS:
--   → colocar en BASE_PATH la ruta absoluta a la carpeta BaseDatos
--		Ejemplo:
--		:setvar BASE_PATH "C:\PROYECTOS\AgropeTech\Proyecto_DAWA_Backend\BaseDatos\"
--   → Importante: No olvidar el "\" al final
-- =====================================================
:setvar BASE_PATH ""

USE master;
GO

PRINT 'Eliminando base de datos si existe...';
DROP DATABASE IF EXISTS [$(DatabaseName)];
GO

PRINT 'Creando base de datos...';
CREATE DATABASE [$(DatabaseName)];
GO

USE [$(DatabaseName)];
GO

PRINT 'Base de datos creada y seleccionada.';
GO

-- ============================================
-- CREACIÓN DE TABLAS
-- ============================================
PRINT 'Creando tablas...';
GO
:r $(BASE_PATH)tables\tables.sql
GO

-- ============================================
-- STORED PROCEDURES
-- ============================================
PRINT 'Creando stored procedures de usuarios...';
GO
:r $(BASE_PATH)stored_procedures\sp_usuarios.sql
GO

PRINT 'Creando stored procedures de archivos...';
GO
:r $(BASE_PATH)stored_procedures\sp_archivos.sql
GO

PRINT 'Creando stored procedures de foros...';
GO
:r $(BASE_PATH)stored_procedures\sp_foros.sql
GO

-- ============================================
-- DATA DEMO (Omitir si se desea)
-- ============================================
PRINT 'Insertando Usuarios demo...';
GO
:r $(BASE_PATH)data_demo\usuarios_demo.sql
GO

PRINT 'Instalación finalizada.';
GO

USE master;
GO