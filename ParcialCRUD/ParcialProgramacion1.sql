IF DB_ID('ParcialProgramacion1') IS NULL
BEGIN
    CREATE DATABASE ParcialProgramacion1;
END;
GO

USE ParcialProgramacion1;
GO

CREATE TABLE dbo.Productos
(
    IdProducto  INT IDENTITY(1,1) PRIMARY KEY,
    Nombre      NVARCHAR(100) NOT NULL,
    Categoria   NVARCHAR(50)  NOT NULL,
    Precio      DECIMAL(10,2) NOT NULL CHECK (Precio > 0),
    Existencias INT           NOT NULL CHECK (Existencias >= 0)
);
GO

INSERT INTO dbo.Productos (Nombre, Categoria, Precio, Existencias) VALUES
('Laptop HP 15', 'Tecnología', 549.99, 10),
('Mouse inalámbrico', 'Accesorios', 12.50, 40),
('Silla ergonómica', 'Oficina', 89.90, 15);