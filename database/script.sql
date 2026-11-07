/*
    Base de datos de Gestión de Productos (SQL Server).

    Crea la base, sus tablas y los datos iniciales: los roles, los tipos de producto,
    dos usuarios de prueba y algunos productos.

    Usuarios de prueba:
      admin@demo.com    / Admin123!     (rol Admin: puede crear, editar y eliminar)
      usuario@demo.com  / Usuario123!   (rol User: solo puede consultar)

    Se puede ejecutar más de una vez: si la base ya existe, la borra y la vuelve a crear.
*/

USE master;
GO

IF DB_ID(N'GestionProductos') IS NOT NULL
BEGIN
    ALTER DATABASE GestionProductos SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE GestionProductos;
END
GO

CREATE DATABASE GestionProductos;
GO

USE GestionProductos;
GO

/* ---------- Tablas ---------- */

CREATE TABLE Roles (
    Id   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,
    Name NVARCHAR(50)      NOT NULL CONSTRAINT UQ_Roles_Name UNIQUE
);
GO

-- La contraseña nunca se guarda en texto plano: solo su hash BCrypt.
CREATE TABLE Users (
    Id           INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
    Name         NVARCHAR(100)     NOT NULL,
    Email        NVARCHAR(256)     NOT NULL CONSTRAINT UQ_Users_Email UNIQUE,
    PasswordHash NVARCHAR(100)     NOT NULL,
    RoleId       INT               NOT NULL CONSTRAINT FK_Users_Roles REFERENCES Roles(Id),
    CreatedAt    DATETIME2         NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME()
);
GO

CREATE TABLE ProductTypes (
    Id   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ProductTypes PRIMARY KEY,
    Name NVARCHAR(100)     NOT NULL CONSTRAINT UQ_ProductTypes_Name UNIQUE
);
GO

CREATE TABLE Products (
    Id            INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Products PRIMARY KEY,
    Name          NVARCHAR(100)     NOT NULL,
    Description   NVARCHAR(500)     NULL,
    Price         DECIMAL(18,2)     NOT NULL CONSTRAINT CK_Products_Price CHECK (Price > 0),
    Stock         INT               NOT NULL CONSTRAINT CK_Products_Stock CHECK (Stock >= 0),
    ProductTypeId INT               NOT NULL CONSTRAINT FK_Products_ProductTypes REFERENCES ProductTypes(Id),
    CreatedAt     DATETIME2         NOT NULL CONSTRAINT DF_Products_CreatedAt DEFAULT SYSUTCDATETIME(),
    UpdatedAt     DATETIME2         NULL
);
GO

CREATE INDEX IX_Products_ProductTypeId ON Products(ProductTypeId);
GO

/* ---------- Datos iniciales ---------- */

INSERT INTO Roles (Name) VALUES (N'Admin'), (N'User');

INSERT INTO Users (Name, Email, PasswordHash, RoleId) VALUES
    (N'Administrador',   N'admin@demo.com',   N'$2a$11$PBa0PCjF3Alr85gRR646lOCihKGh7dCx44f9H6x4WzzOlM//48QEy', 1),
    (N'Usuario de prueba', N'usuario@demo.com', N'$2a$11$WYpBL8CuXj3zNdFA4rbJDOPotOP8BwC2bHnO4mABd/1zQVElCq.9W', 2);

INSERT INTO ProductTypes (Name) VALUES
    (N'Electrónica'), (N'Hogar'), (N'Alimentos'), (N'Ropa'), (N'Oficina');

INSERT INTO Products (Name, Description, Price, Stock, ProductTypeId) VALUES
    (N'Audífonos inalámbricos', N'Audífonos Bluetooth con estuche de carga', 45.99, 30, 1),
    (N'Teclado mecánico',       N'Teclado con iluminación y switches azules', 69.50, 12, 1),
    (N'Licuadora',              N'Licuadora de 5 velocidades, vaso de vidrio', 38.00,  8, 2),
    (N'Café molido 500 g',      N'Café de altura, tueste medio',               6.75,  50, 3),
    (N'Camiseta de algodón',    NULL,                                          12.00, 40, 4),
    (N'Resma de papel carta',   N'500 hojas de 75 g',                           5.25, 100, 5);
GO
