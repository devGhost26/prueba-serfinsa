/*
    Usuario con el que se conecta la API en Docker. No es "sa": solo puede leer y escribir
    datos, no crear ni borrar tablas, ni tocar otras bases. Si alguien abusara de la API,
    no podría hacer más que eso.

    La contraseña llega desde db-init.sh como variable de sqlcmd: $(APP_PASSWORD).
*/
SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'gestion_app')
    CREATE LOGIN gestion_app WITH PASSWORD = N'$(APP_PASSWORD)', CHECK_POLICY = ON, DEFAULT_DATABASE = GestionProductos;
ELSE
    ALTER LOGIN gestion_app WITH PASSWORD = N'$(APP_PASSWORD)';
GO

USE GestionProductos;
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'gestion_app')
    CREATE USER gestion_app FOR LOGIN gestion_app;

ALTER ROLE db_datareader ADD MEMBER gestion_app;
ALTER ROLE db_datawriter ADD MEMBER gestion_app;
GO
