CREATE DATABASE SharksWeb;
GO
USE SharksWeb;
GO

CREATE TABLE Rol (
    id INT PRIMARY KEY IDENTITY,
    nombre NVARCHAR(255) NOT NULL,
    permisos NVARCHAR(MAX)
);
GO

CREATE TABLE tipo_egreso (
    id INT PRIMARY KEY IDENTITY,
    nombre NVARCHAR(255) NOT NULL
);
GO

CREATE TABLE tipo_ingreso (
    id INT PRIMARY KEY IDENTITY,
    nombre NVARCHAR(255) NOT NULL
);
GO

CREATE TABLE Proveedor (
    id INT PRIMARY KEY IDENTITY,
    nombre NVARCHAR(255) NOT NULL,
    direccion NVARCHAR(255),
    telefono NVARCHAR(255),
    contacto_correo NVARCHAR(255)
);
GO

CREATE TABLE Categoria (
    id INT PRIMARY KEY IDENTITY,
    nombre NVARCHAR(255) NOT NULL,
    descripcion NVARCHAR(MAX)
);
GO

CREATE TABLE Users (
    id INT PRIMARY KEY IDENTITY,
    nombre NVARCHAR(255) NOT NULL,
    apellido_paterno NVARCHAR(255),
    apellido_materno NVARCHAR(255),
    id_rol INT NOT NULL FOREIGN KEY REFERENCES Rol(id),
    email NVARCHAR(255) UNIQUE NOT NULL,
    password NVARCHAR(255) NOT NULL
);
GO

CREATE TABLE Cliente (
    id INT PRIMARY KEY IDENTITY,
    nombre NVARCHAR(255) NOT NULL,
    apellido_paterno NVARCHAR(255),
    apellido_materno NVARCHAR(255),
    ci NVARCHAR(255) UNIQUE NOT NULL,
    celular NVARCHAR(255),
    sharkCoins DECIMAL(18,2) DEFAULT 0,
    deuda DECIMAL(18,2) DEFAULT 0
);
GO

CREATE TABLE Producto (	
    id INT PRIMARY KEY IDENTITY,
    nombre NVARCHAR(255) NOT NULL,
    descripcion NVARCHAR(MAX),
    id_categoria INT NOT NULL FOREIGN KEY REFERENCES Categoria(id),
    precio_compra DECIMAL(18,2),
    precio_venta DECIMAL(18,2),
    precio_sharkcoins DECIMAL(18,2)
);
GO

CREATE TABLE Stock (
    id INT PRIMARY KEY IDENTITY,
    id_producto INT NOT NULL FOREIGN KEY REFERENCES Producto(id),
    cantidad INT NOT NULL,
    tipo_movimiento NVARCHAR(20) CHECK (tipo_movimiento IN ('Entrada', 'Salida', 'Ajuste')),
    fecha_movimiento DATETIME2 DEFAULT GETDATE(),
    descripcion NVARCHAR(MAX)
);
GO

CREATE TABLE Compra (
    id INT PRIMARY KEY IDENTITY,
    id_usuario INT NOT NULL FOREIGN KEY REFERENCES Users(id),
    fecha_compra DATE DEFAULT GETDATE()
);
GO

CREATE TABLE Compra_Proveedor (
    id_compra INT NOT NULL FOREIGN KEY REFERENCES Compra(id),
    id_proveedor INT NOT NULL FOREIGN KEY REFERENCES Proveedor(id),
    nombre_producto NVARCHAR(255) NOT NULL,
    cantidad INT NOT NULL,
    precio_unitario DECIMAL(18,2) NOT NULL,
    PRIMARY KEY (id_compra, id_proveedor)
);
GO

CREATE TABLE Venta (
    id INT PRIMARY KEY IDENTITY,
    id_cliente INT NOT NULL FOREIGN KEY REFERENCES Cliente(id),
    id_usuario INT NOT NULL FOREIGN KEY REFERENCES Users(id),
    fecha_venta DATE DEFAULT GETDATE(),
	total DECIMAL(18,2) NOT NULL,
    descuento DECIMAL(18,2) NOT NULL ,
    pago DECIMAL(18,2) NOT NULL 
);
GO

CREATE TABLE Producto_Venta (
    id_producto INT NOT NULL FOREIGN KEY REFERENCES Producto(id),
    id_venta INT NOT NULL FOREIGN KEY REFERENCES Venta(id),
    cantidad INT NOT NULL,
    precio_unitario DECIMAL(18,2) NOT NULL,
    tipo_pago NVARCHAR(50) NOT NULL,
    PRIMARY KEY (id_producto, id_venta)
);
GO

CREATE TABLE Preventa (
    id INT PRIMARY KEY IDENTITY,
    id_cliente INT NOT NULL FOREIGN KEY REFERENCES Cliente(id),
    id_usuario INT NOT NULL FOREIGN KEY REFERENCES Users(id),
    fecha_preventa DATE DEFAULT GETDATE(),
    total DECIMAL(18,2) NOT NULL,
    tipo_pago NVARCHAR(50) NOT NULL,
    descripcion NVARCHAR(MAX)
);
GO

CREATE TABLE Torneo (
    id INT PRIMARY KEY IDENTITY,
    nombre NVARCHAR(255) NOT NULL,
    fecha DATE NOT NULL,
	entrada DECIMAL(18,2) NOT NULL ,
	estado NVARCHAR(50) NOT NULL
);
GO

CREATE TABLE Torneo_Cliente (
    id_torneo INT NOT NULL FOREIGN KEY REFERENCES Torneo(id),
    id_cliente INT NOT NULL FOREIGN KEY REFERENCES Cliente(id),
    pago DECIMAL(18,2) NOT NULL,
    tipo_pago NVARCHAR(50) NOT NULL,
    PRIMARY KEY (id_torneo, id_cliente)
);
GO

CREATE TABLE Ganadores_Torneo (
    id INT PRIMARY KEY IDENTITY,
    id_torneo INT NOT NULL,
    id_cliente INT NOT NULL,
    premio_sharkcoins DECIMAL(18,2) NOT NULL,
	puesto INT NOT NULL
    FOREIGN KEY (id_torneo, id_cliente) REFERENCES Torneo_Cliente(id_torneo, id_cliente)
);
GO

CREATE TABLE Egreso (
    id INT PRIMARY KEY IDENTITY,
    id_tipo_egreso INT NOT NULL FOREIGN KEY REFERENCES tipo_egreso(id),
    id_usuario INT NOT NULL FOREIGN KEY REFERENCES Users(id),
    monto DECIMAL(18,2) NOT NULL,
    fecha DATE DEFAULT GETDATE(),
    descripcion NVARCHAR(MAX)
);
GO

CREATE TABLE Ingreso (
    id INT PRIMARY KEY IDENTITY,
    id_tipo_ingreso INT NOT NULL FOREIGN KEY REFERENCES tipo_ingreso(id),
    id_cliente INT NOT NULL FOREIGN KEY REFERENCES Cliente(id),
    monto DECIMAL(18,2) NOT NULL,
    fecha DATE DEFAULT GETDATE(),
    descripcion NVARCHAR(MAX)
);
GO

CREATE TABLE Deuda (
    id INT PRIMARY KEY IDENTITY,
    id_cliente INT NOT NULL FOREIGN KEY REFERENCES Cliente(id),
    monto DECIMAL(18,2) NOT NULL,
    tipoDeuda NVARCHAR(255),
    descripcion NVARCHAR(MAX),
    created_at DATETIME2 DEFAULT GETDATE()
);
GO

CREATE TABLE codigo_tcg (
    id INT PRIMARY KEY IDENTITY,
    id_cliente INT NOT NULL FOREIGN KEY REFERENCES Cliente(id),
    juego NVARCHAR(255) NOT NULL,
    codigo NVARCHAR(255) NOT NULL
);
GO

CREATE TABLE Sharkcoin (
    id INT PRIMARY KEY IDENTITY,
    id_cliente INT NOT NULL FOREIGN KEY REFERENCES Cliente(id),
    monto DECIMAL(18,2) NOT NULL,
    tipo NVARCHAR(255) CHECK (tipo IN ('Asignación', 'Redención')),
    fecha DATETIME2 DEFAULT GETDATE()
);
GO


