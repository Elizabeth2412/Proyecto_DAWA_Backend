USE AgroPetech;
GO
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
    cursoId INT NULL,
    titulo VARCHAR(200) NOT NULL,
    modulo VARCHAR(100),
    totalPreguntas INT DEFAULT 0,
    duracion VARCHAR(50),
    estado VARCHAR(20) CHECK (estado IN ('Activa', 'Inactiva')) DEFAULT 'Activa',

    fechaCreacion DATETIME NULL DEFAULT GETDATE(),
    usuarioCreacionId INT NULL,

    fechaModificacion DATETIME NULL,
    usuarioModificacionId INT NULL,

    fechaEliminacion DATETIME NULL,
    usuarioEliminacionId INT NULL,

    CONSTRAINT FK_Evaluacion_Curso FOREIGN KEY (cursoId) REFERENCES Curso(id),
    CONSTRAINT FK_Evaluacion_UsuarioCreacion FOREIGN KEY (usuarioCreacionId) REFERENCES Usuario(id),
    CONSTRAINT FK_Evaluacion_UsuarioModificacion FOREIGN KEY (usuarioModificacionId) REFERENCES Usuario(id),
    CONSTRAINT FK_Evaluacion_UsuarioEliminacion FOREIGN KEY (usuarioEliminacionId) REFERENCES Usuario(id)
);
GO

-- Tabla de Preguntas
CREATE TABLE Pregunta (
    id INT IDENTITY(1,1) PRIMARY KEY,
    evaluacionId INT NULL,
    texto VARCHAR(500) NOT NULL,
    estado VARCHAR(20) CHECK (estado IN ('Activa', 'Inactiva')) DEFAULT 'Activa',

    usuarioCreacionId INT NOT NULL,
    fechaCreacion DATETIME DEFAULT GETDATE(),

    usuarioModificacionId INT NULL,
    fechaModificacion DATETIME NULL,

    usuarioEliminacionId INT NULL,
    fechaEliminacion DATETIME NULL,

    CONSTRAINT FK_Pregunta_Evaluacion FOREIGN KEY (evaluacionId) REFERENCES Evaluacion(id),
    CONSTRAINT FK_Pregunta_UsuarioCreacion FOREIGN KEY (usuarioCreacionId) REFERENCES Usuario(id),
    CONSTRAINT FK_Pregunta_UsuarioModificacion FOREIGN KEY (usuarioModificacionId) REFERENCES Usuario(id),
    CONSTRAINT FK_Pregunta_UsuarioEliminacion FOREIGN KEY (usuarioEliminacionId) REFERENCES Usuario(id)
);
GO


-- Tabla de Opciones de Preguntas
CREATE TABLE Opcion (
    id INT IDENTITY(1,1) PRIMARY KEY,
    preguntaId INT NULL,
    texto VARCHAR(200) NOT NULL,
    esCorrecta BIT DEFAULT 0 NULL,
    estado VARCHAR(20) CHECK (estado IN ('Activa', 'Inactiva')) DEFAULT 'Activa',

    usuarioCreacionId INT NULL,
    fechaCreacion DATETIME NULL DEFAULT GETDATE() ,

    usuarioModificacionId INT NULL,
    fechaModificacion DATETIME NULL,

    usuarioEliminacionId INT NULL,
    fechaEliminacion DATETIME NULL,

    CONSTRAINT FK_Opcion_Pregunta FOREIGN KEY (preguntaId) REFERENCES Pregunta(id),
    CONSTRAINT FK_Opcion_UsuarioCreacion FOREIGN KEY (usuarioCreacionId) REFERENCES Usuario(id),

    CONSTRAINT FK_Opcion_UsuarioModificacion FOREIGN KEY (usuarioModificacionId) REFERENCES Usuario(id),

    CONSTRAINT FK_Opcion_UsuarioEliminacion FOREIGN KEY (usuarioEliminacionId) REFERENCES Usuario(id)
);
GO


-- Tabla de foros
CREATE TABLE PublicacionesForo (
    Id BIGINT IDENTITY(1,1) NOT NULL,

    ParentId BIGINT NULL,
    RootId BIGINT NULL,

    Titulo NVARCHAR(200) NULL,
    Contenido NVARCHAR(MAX) NOT NULL,
    UrlImagen VARCHAR(500) NULL,

    UsuarioId INT NOT NULL,

    FechaCreacion DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    FechaModificacion DATETIME2 NULL,
    UsuarioModificacionId INT NULL,
    FechaEliminacion DATETIME2 NULL,
    UsuarioEliminacionId INT NULL,
    Estado BIT NOT NULL DEFAULT 1,

    CONSTRAINT PK_PublicacionesForo 
        PRIMARY KEY CLUSTERED (Id)
);
GO

ALTER TABLE PublicacionesForo
ADD CONSTRAINT FK_Publicaciones_Parent
FOREIGN KEY (ParentId) REFERENCES PublicacionesForo(Id);
GO

ALTER TABLE PublicacionesForo
ADD CONSTRAINT FK_Publicaciones_Usuario
FOREIGN KEY (UsuarioId) REFERENCES Usuario(Id);
GO

ALTER TABLE PublicacionesForo
ADD CONSTRAINT FK_Publicaciones_UsuarioModificacion
FOREIGN KEY (UsuarioModificacionId) REFERENCES Usuario(Id);
GO

ALTER TABLE PublicacionesForo
ADD CONSTRAINT FK_Publicaciones_UsuarioEliminacion
FOREIGN KEY (UsuarioEliminacionId) REFERENCES Usuario(Id);
GO

-- Crear indices para optimizar consultas
CREATE NONCLUSTERED INDEX IX_Publicaciones_RootId
ON PublicacionesForo(RootId)
INCLUDE (Estado, FechaCreacion)
WHERE Estado = 1;
GO

CREATE NONCLUSTERED INDEX IX_Publicaciones_ParentId
ON PublicacionesForo(ParentId)
INCLUDE (Estado, FechaCreacion)
WHERE Estado = 1;
GO

CREATE NONCLUSTERED INDEX IX_Publicaciones_UsuarioId
ON PublicacionesForo(UsuarioId, Estado, FechaCreacion DESC);
GO

CREATE NONCLUSTERED INDEX IX_Publicaciones_Fecha
ON PublicacionesForo(FechaCreacion DESC)
INCLUDE (Titulo, UsuarioId, ParentId, RootId)
WHERE Estado = 1 AND ParentId IS NULL;
GO
-- Crear indice de texto completo para busquedas en Titulo y Contenido
IF NOT EXISTS (
    SELECT 1 
    FROM sys.fulltext_catalogs 
    WHERE name = 'FTC_Publicaciones'
)
BEGIN
    CREATE FULLTEXT CATALOG FTC_Publicaciones
    WITH ACCENT_SENSITIVITY = OFF;
END
GO

/*
CREATE FULLTEXT INDEX ON PublicacionesForo
(
    Titulo LANGUAGE 'Spanish',
    Contenido LANGUAGE 'Spanish'
)
KEY INDEX PK_PublicacionesForo
ON FTC_Publicaciones
WITH CHANGE_TRACKING AUTO;
GO
*/