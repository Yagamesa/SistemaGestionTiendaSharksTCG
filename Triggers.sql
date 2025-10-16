-- SCRIPT DE TRIGGERS DE AUDITORIA PARA TODAS LAS TABLAS

-- 1. Trigger para INSERT en tabla Rol
CREATE TRIGGER trg_Auditoria_INSERT_Rol
ON Rol
AFTER INSERT
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Rol',
        'INSERT',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 2. Trigger para UPDATE en tabla Rol
CREATE TRIGGER trg_Auditoria_UPDATE_Rol
ON Rol
AFTER UPDATE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Rol',
        'UPDATE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 3. Trigger para DELETE en tabla Rol
CREATE TRIGGER trg_Auditoria_DELETE_Rol
ON Rol
AFTER DELETE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Rol',
        'DELETE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM deleted FOR JSON PATH);
END;

-- 4. Trigger para INSERT en tabla tipo_egreso
CREATE TRIGGER trg_Auditoria_INSERT_tipo_egreso
ON tipo_egreso
AFTER INSERT
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'tipo_egreso',
        'INSERT',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 5. Trigger para UPDATE en tabla tipo_egreso
CREATE TRIGGER trg_Auditoria_UPDATE_tipo_egreso
ON tipo_egreso
AFTER UPDATE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'tipo_egreso',
        'UPDATE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 6. Trigger para DELETE en tabla tipo_egreso
CREATE TRIGGER trg_Auditoria_DELETE_tipo_egreso
ON tipo_egreso
AFTER DELETE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'tipo_egreso',
        'DELETE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM deleted FOR JSON PATH);
END;

-- 7. Trigger para INSERT en tabla tipo_ingreso
CREATE TRIGGER trg_Auditoria_INSERT_tipo_ingreso
ON tipo_ingreso
AFTER INSERT
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'tipo_ingreso',
        'INSERT',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 8. Trigger para UPDATE en tabla tipo_ingreso
CREATE TRIGGER trg_Auditoria_UPDATE_tipo_ingreso
ON tipo_ingreso
AFTER UPDATE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'tipo_ingreso',
        'UPDATE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 9. Trigger para DELETE en tabla tipo_ingreso
CREATE TRIGGER trg_Auditoria_DELETE_tipo_ingreso
ON tipo_ingreso
AFTER DELETE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'tipo_ingreso',
        'DELETE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM deleted FOR JSON PATH);
END;

-- 10. Trigger para INSERT en tabla Proveedor
CREATE TRIGGER trg_Auditoria_INSERT_Proveedor
ON Proveedor
AFTER INSERT
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Proveedor',
        'INSERT',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 11. Trigger para UPDATE en tabla Proveedor
CREATE TRIGGER trg_Auditoria_UPDATE_Proveedor
ON Proveedor
AFTER UPDATE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Proveedor',
        'UPDATE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 12. Trigger para DELETE en tabla Proveedor
CREATE TRIGGER trg_Auditoria_DELETE_Proveedor
ON Proveedor
AFTER DELETE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Proveedor',
        'DELETE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM deleted FOR JSON PATH);
END;

-- 13. Trigger para INSERT en tabla Categoria
CREATE TRIGGER trg_Auditoria_INSERT_Categoria
ON Categoria
AFTER INSERT
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Categoria',
        'INSERT',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 14. Trigger para UPDATE en tabla Categoria
CREATE TRIGGER trg_Auditoria_UPDATE_Categoria
ON Categoria
AFTER UPDATE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Categoria',
        'UPDATE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 15. Trigger para DELETE en tabla Categoria
CREATE TRIGGER trg_Auditoria_DELETE_Categoria
ON Categoria
AFTER DELETE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Categoria',
        'DELETE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM deleted FOR JSON PATH);
END;

-- 16. Trigger para INSERT en tabla Users
CREATE TRIGGER trg_Auditoria_INSERT_Users
ON Users
AFTER INSERT
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Users',
        'INSERT',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 17. Trigger para UPDATE en tabla Users
CREATE TRIGGER trg_Auditoria_UPDATE_Users
ON Users
AFTER UPDATE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Users',
        'UPDATE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 18. Trigger para DELETE en tabla Users
CREATE TRIGGER trg_Auditoria_DELETE_Users
ON Users
AFTER DELETE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Users',
        'DELETE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM deleted FOR JSON PATH);
END;

-- 19. Trigger para INSERT en tabla Cliente
CREATE TRIGGER trg_Auditoria_INSERT_Cliente
ON Cliente
AFTER INSERT
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Cliente',
        'INSERT',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 20. Trigger para UPDATE en tabla Cliente
CREATE TRIGGER trg_Auditoria_UPDATE_Cliente
ON Cliente
AFTER UPDATE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Cliente',
        'UPDATE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 21. Trigger para DELETE en tabla Cliente
CREATE TRIGGER trg_Auditoria_DELETE_Cliente
ON Cliente
AFTER DELETE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Cliente',
        'DELETE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM deleted FOR JSON PATH);
END;

-- 22. Trigger para INSERT en tabla Producto
CREATE TRIGGER trg_Auditoria_INSERT_Producto
ON Producto
AFTER INSERT
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Producto',
        'INSERT',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 23. Trigger para UPDATE en tabla Producto
CREATE TRIGGER trg_Auditoria_UPDATE_Producto
ON Producto
AFTER UPDATE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Producto',
        'UPDATE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 24. Trigger para DELETE en tabla Producto
CREATE TRIGGER trg_Auditoria_DELETE_Producto
ON Producto
AFTER DELETE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Producto',
        'DELETE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM deleted FOR JSON PATH);
END;

-- 25. Trigger para INSERT en tabla Stock
CREATE TRIGGER trg_Auditoria_INSERT_Stock
ON Stock
AFTER INSERT
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Stock',
        'INSERT',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 26. Trigger para UPDATE en tabla Stock
CREATE TRIGGER trg_Auditoria_UPDATE_Stock
ON Stock
AFTER UPDATE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Stock',
        'UPDATE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 27. Trigger para DELETE en tabla Stock
CREATE TRIGGER trg_Auditoria_DELETE_Stock
ON Stock
AFTER DELETE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Stock',
        'DELETE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM deleted FOR JSON PATH);
END;

-- 28. Trigger para INSERT en tabla Compra
CREATE TRIGGER trg_Auditoria_INSERT_Compra
ON Compra
AFTER INSERT
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Compra',
        'INSERT',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 29. Trigger para UPDATE en tabla Compra
CREATE TRIGGER trg_Auditoria_UPDATE_Compra
ON Compra
AFTER UPDATE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Compra',
        'UPDATE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 30. Trigger para DELETE en tabla Compra
CREATE TRIGGER trg_Auditoria_DELETE_Compra
ON Compra
AFTER DELETE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Compra',
        'DELETE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM deleted FOR JSON PATH);
END;

-- 31. Trigger para INSERT en tabla Compra_Proveedor
CREATE TRIGGER trg_Auditoria_INSERT_Compra_Proveedor
ON Compra_Proveedor
AFTER INSERT
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Compra_Proveedor',
        'INSERT',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 32. Trigger para UPDATE en tabla Compra_Proveedor
CREATE TRIGGER trg_Auditoria_UPDATE_Compra_Proveedor
ON Compra_Proveedor
AFTER UPDATE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Compra_Proveedor',
        'UPDATE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 33. Trigger para DELETE en tabla Compra_Proveedor
CREATE TRIGGER trg_Auditoria_DELETE_Compra_Proveedor
ON Compra_Proveedor
AFTER DELETE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Compra_Proveedor',
        'DELETE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM deleted FOR JSON PATH);
END;

-- 34. Trigger para INSERT en tabla Venta
CREATE TRIGGER trg_Auditoria_INSERT_Venta
ON Venta
AFTER INSERT
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Venta',
        'INSERT',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 35. Trigger para UPDATE en tabla Venta
CREATE TRIGGER trg_Auditoria_UPDATE_Venta
ON Venta
AFTER UPDATE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Venta',
        'UPDATE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 36. Trigger para DELETE en tabla Venta
CREATE TRIGGER trg_Auditoria_DELETE_Venta
ON Venta
AFTER DELETE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Venta',
        'DELETE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM deleted FOR JSON PATH);
END;

-- 37. Trigger para INSERT en tabla Producto_Venta
CREATE TRIGGER trg_Auditoria_INSERT_Producto_Venta
ON Producto_Venta
AFTER INSERT
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Producto_Venta',
        'INSERT',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 38. Trigger para UPDATE en tabla Producto_Venta
CREATE TRIGGER trg_Auditoria_UPDATE_Producto_Venta
ON Producto_Venta
AFTER UPDATE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Producto_Venta',
        'UPDATE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 39. Trigger para DELETE en tabla Producto_Venta
CREATE TRIGGER trg_Auditoria_DELETE_Producto_Venta
ON Producto_Venta
AFTER DELETE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Producto_Venta',
        'DELETE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM deleted FOR JSON PATH);
END;

-- 40. Trigger para INSERT en tabla Preventa
CREATE TRIGGER trg_Auditoria_INSERT_Preventa
ON Preventa
AFTER INSERT
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Preventa',
        'INSERT',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 41. Trigger para UPDATE en tabla Preventa
CREATE TRIGGER trg_Auditoria_UPDATE_Preventa
ON Preventa
AFTER UPDATE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Preventa',
        'UPDATE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 42. Trigger para DELETE en tabla Preventa
CREATE TRIGGER trg_Auditoria_DELETE_Preventa
ON Preventa
AFTER DELETE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Preventa',
        'DELETE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM deleted FOR JSON PATH);
END;

-- 43. Trigger para INSERT en tabla Torneo
CREATE TRIGGER trg_Auditoria_INSERT_Torneo
ON Torneo
AFTER INSERT
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Torneo',
        'INSERT',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 44. Trigger para UPDATE en tabla Torneo
CREATE TRIGGER trg_Auditoria_UPDATE_Torneo
ON Torneo
AFTER UPDATE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Torneo',
        'UPDATE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 45. Trigger para DELETE en tabla Torneo
CREATE TRIGGER trg_Auditoria_DELETE_Torneo
ON Torneo
AFTER DELETE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Torneo',
        'DELETE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM deleted FOR JSON PATH);
END;

-- 46. Trigger para INSERT en tabla Torneo_Cliente
CREATE TRIGGER trg_Auditoria_INSERT_Torneo_Cliente
ON Torneo_Cliente
AFTER INSERT
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Torneo_Cliente',
        'INSERT',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 47. Trigger para UPDATE en tabla Torneo_Cliente
CREATE TRIGGER trg_Auditoria_UPDATE_Torneo_Cliente
ON Torneo_Cliente
AFTER UPDATE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Torneo_Cliente',
        'UPDATE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 48. Trigger para DELETE en tabla Torneo_Cliente
CREATE TRIGGER trg_Auditoria_DELETE_Torneo_Cliente
ON Torneo_Cliente
AFTER DELETE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Torneo_Cliente',
        'DELETE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM deleted FOR JSON PATH);
END;

-- 49. Trigger para INSERT en tabla Ganadores_Torneo
CREATE TRIGGER trg_Auditoria_INSERT_Ganadores_Torneo
ON Ganadores_Torneo
AFTER INSERT
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Ganadores_Torneo',
        'INSERT',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 50. Trigger para UPDATE en tabla Ganadores_Torneo
CREATE TRIGGER trg_Auditoria_UPDATE_Ganadores_Torneo
ON Ganadores_Torneo
AFTER UPDATE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Ganadores_Torneo',
        'UPDATE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 51. Trigger para DELETE en tabla Ganadores_Torneo
CREATE TRIGGER trg_Auditoria_DELETE_Ganadores_Torneo
ON Ganadores_Torneo
AFTER DELETE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Ganadores_Torneo',
        'DELETE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM deleted FOR JSON PATH);
END;

-- 52. Trigger para INSERT en tabla Egreso
CREATE TRIGGER trg_Auditoria_INSERT_Egreso
ON Egreso
AFTER INSERT
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Egreso',
        'INSERT',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 53. Trigger para UPDATE en tabla Egreso
CREATE TRIGGER trg_Auditoria_UPDATE_Egreso
ON Egreso
AFTER UPDATE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Egreso',
        'UPDATE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 54. Trigger para DELETE en tabla Egreso
CREATE TRIGGER trg_Auditoria_DELETE_Egreso
ON Egreso
AFTER DELETE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Egreso',
        'DELETE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM deleted FOR JSON PATH);
END;

-- 55. Trigger para INSERT en tabla Ingreso
CREATE TRIGGER trg_Auditoria_INSERT_Ingreso
ON Ingreso
AFTER INSERT
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Ingreso',
        'INSERT',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 56. Trigger para UPDATE en tabla Ingreso
CREATE TRIGGER trg_Auditoria_UPDATE_Ingreso
ON Ingreso
AFTER UPDATE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Ingreso',
        'UPDATE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 57. Trigger para DELETE en tabla Ingreso
CREATE TRIGGER trg_Auditoria_DELETE_Ingreso
ON Ingreso
AFTER DELETE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Ingreso',
        'DELETE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM deleted FOR JSON PATH);
END;

-- 58. Trigger para INSERT en tabla Deuda
CREATE TRIGGER trg_Auditoria_INSERT_Deuda
ON Deuda
AFTER INSERT
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Deuda',
        'INSERT',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 59. Trigger para UPDATE en tabla Deuda
CREATE TRIGGER trg_Auditoria_UPDATE_Deuda
ON Deuda
AFTER UPDATE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Deuda',
        'UPDATE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 60. Trigger para DELETE en tabla Deuda
CREATE TRIGGER trg_Auditoria_DELETE_Deuda
ON Deuda
AFTER DELETE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Deuda',
        'DELETE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM deleted FOR JSON PATH);
END;

-- 61. Trigger para INSERT en tabla codigo_tcg
CREATE TRIGGER trg_Auditoria_INSERT_codigo_tcg
ON codigo_tcg
AFTER INSERT
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'codigo_tcg',
        'INSERT',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 62. Trigger para UPDATE en tabla codigo_tcg
CREATE TRIGGER trg_Auditoria_UPDATE_codigo_tcg
ON codigo_tcg
AFTER UPDATE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'codigo_tcg',
        'UPDATE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 63. Trigger para DELETE en tabla codigo_tcg
CREATE TRIGGER trg_Auditoria_DELETE_codigo_tcg
ON codigo_tcg
AFTER DELETE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'codigo_tcg',
        'DELETE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM deleted FOR JSON PATH);
END;

-- 64. Trigger para INSERT en tabla Sharkcoin
CREATE TRIGGER trg_Auditoria_INSERT_Sharkcoin
ON Sharkcoin
AFTER INSERT
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Sharkcoin',
        'INSERT',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 65. Trigger para UPDATE en tabla Sharkcoin
CREATE TRIGGER trg_Auditoria_UPDATE_Sharkcoin
ON Sharkcoin
AFTER UPDATE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Sharkcoin',
        'UPDATE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM inserted FOR JSON PATH);
END;

-- 66. Trigger para DELETE en tabla Sharkcoin
CREATE TRIGGER trg_Auditoria_DELETE_Sharkcoin
ON Sharkcoin
AFTER DELETE
AS
BEGIN
    INSERT INTO Auditoria (Tabla, Accion, Fecha, Usuario, Datos)
    SELECT
        'Sharkcoin',
        'DELETE',
        GETDATE(),
        SYSTEM_USER,
        (SELECT * FROM deleted FOR JSON PATH);
END;

select * from Auditoria