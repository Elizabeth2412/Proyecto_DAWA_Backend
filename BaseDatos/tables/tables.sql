-- Tabla de Usuarios
CREATE TABLE Usuario (
    id INT IDENTITY(1,1) PRIMARY KEY,
    email VARCHAR(100) UNIQUE NOT NULL,
    password VARCHAR(100) NOT NULL,
    tipo VARCHAR(20) CHECK (tipo IN ('administrador', 'instructor', 'estudiante')),
    nombre VARCHAR(100) NOT NULL,
    apellido VARCHAR(100),
    edad INT
);
GO

-- Tabla de Cursos
CREATE TABLE Curso (
    id INT IDENTITY(1,1) PRIMARY KEY,
    titulo VARCHAR(200) NOT NULL,
    descripcion VARCHAR(1000),
    nivel VARCHAR(50),
    duracion INT,
    progreso INT DEFAULT 0,
    instructor VARCHAR(100) NOT NULL,
    fechaCreacion DATETIME DEFAULT GETDATE(),
    fechaActualizacion DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (instructor) REFERENCES Usuario(email)
);
GO

-- Tabla de Archivos
CREATE TABLE Archivo (
    id INT IDENTITY(1,1) PRIMARY KEY,
    nombre VARCHAR(200) NOT NULL,
    tipo VARCHAR(20) CHECK (tipo IN ('PDF', 'PPTX', 'DOCX')),
    tamano BIGINT NOT NULL,
    contenido VARBINARY(MAX),
    descripcion VARCHAR(500),
    usuario VARCHAR(100) NOT NULL,
    estado VARCHAR(20) CHECK (estado IN ('Disponible', 'NoDisponible')),
    fechaSubida DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (usuario) REFERENCES Usuario(email)
);
GO

-- Tabla intermedia para relacion Curso-Archivo
CREATE TABLE CursoArchivo (
    cursoId INT,
    archivoId INT,
    PRIMARY KEY (cursoId, archivoId),
    FOREIGN KEY (cursoId) REFERENCES Curso(id) ON DELETE CASCADE,
    FOREIGN KEY (archivoId) REFERENCES Archivo(id) ON DELETE CASCADE
);
GO

-- Tabla de Evaluaciones
CREATE TABLE Evaluacion (
    id INT IDENTITY(1,1) PRIMARY KEY,
    titulo VARCHAR(200) NOT NULL,
    modulo VARCHAR(100),
    totalPreguntas INT DEFAULT 0,
    duracion VARCHAR(50),
    fechaCreacion DATETIME DEFAULT GETDATE(),
    estado VARCHAR(20) CHECK (estado IN ('Activa', 'Inactiva'))
);
GO

-- Tabla de Preguntas
CREATE TABLE Pregunta (
    id INT IDENTITY(1,1) PRIMARY KEY,
    evaluacionId INT,
    texto VARCHAR(500) NOT NULL,
    FOREIGN KEY (evaluacionId) REFERENCES Evaluacion(id) ON DELETE CASCADE
);
GO

-- Tabla de Opciones de Preguntas
CREATE TABLE Opcion (
    id INT IDENTITY(1,1) PRIMARY KEY,
    preguntaId INT,
    texto VARCHAR(200) NOT NULL,
    esCorrecta BIT DEFAULT 0,
    FOREIGN KEY (preguntaId) REFERENCES Pregunta(id) ON DELETE CASCADE
);
GO