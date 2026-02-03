USE AgroPetech;
GO
/* ============================================
   Proyecto: AgroPetech
   Script: usuarios_beta.sql
   Descripción: Inserta datos iniciales en la tabla Usuario
   ============================================ */
INSERT INTO Usuario (email, password, tipo, nombre, apellido, edad) 
VALUES 
    ('elizabeth@gmail.com', 'admin123', 'administrador', 'Elizabeth', 'Franco', 20),
    ('leslie@gmail.com', 'instructor123', 'instructor', 'Leslie', 'Vera', 20),
    ('joshua@hotmail.com', 'estudiante123', 'estudiante', 'Josh�a', 'Castillo', 20),
    ('jonacas2000@outlook.com', '123456', 'estudiante', 'Jonathan', 'Castro', 20),
    ('juan@outlook.com', '123456', 'estudiante', 'Juan', 'Robles', 20);
GO
SELECT * FROM Usuario;
GO
GO
PRINT 'Usuarios demo insertados correctamente.';
GO