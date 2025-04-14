-- Script para cambiar la columna Id de la tabla Conversations de string a int con IDENTITY
-- IMPORTANTE: Este script crea una copia temporal de los datos, elimina las tablas originales y las recrea con la nueva estructura

BEGIN TRANSACTION;

-- Paso 1: Crear tabla temporal para guardar los datos de ChatMessages
CREATE TABLE #TempChatMessages (
    Id INT,
    ConversationId INT,
    SenderId NVARCHAR(450),
    Content NVARCHAR(MAX),
    SentAt DATETIME2,
    IsRead BIT
);

-- Paso 2: Crear tabla temporal para guardar los datos de Conversations
CREATE TABLE #TempConversations (
    Id INT IDENTITY(1,1),
    UserId NVARCHAR(450),
    AdminId NVARCHAR(450),
    IsAdminBroadcast BIT,
    IsClosed BIT,
    IsReadByAdmin BIT,
    IsReadByUser BIT,
    CreatedAt DATETIME2,
    LastMessageAt DATETIME2
);

-- Paso 3: Crear un mapeo entre los IDs antiguos (string) y nuevos (int) para Conversations
CREATE TABLE #IdMapping (
    OldId NVARCHAR(450),
    NewId INT
);

-- Paso 4: Copiar los datos de Conversations a la tabla temporal sin la columna identidad
INSERT INTO #TempConversations (UserId, AdminId, IsAdminBroadcast, IsClosed, IsReadByAdmin, IsReadByUser, CreatedAt, LastMessageAt)
SELECT UserId, AdminId, IsAdminBroadcast, IsClosed, IsReadByAdmin, IsReadByUser, CreatedAt, LastMessageAt
FROM Conversations;

-- Paso 5: Guardar el mapeo entre IDs antiguos y nuevos
INSERT INTO #IdMapping (OldId, NewId)
SELECT c.Id, tc.Id
FROM Conversations c
JOIN #TempConversations tc ON 
    c.UserId = tc.UserId AND 
    c.CreatedAt = tc.CreatedAt AND 
    ((c.AdminId IS NULL AND tc.AdminId IS NULL) OR (c.AdminId = tc.AdminId))
ORDER BY c.Id;

-- Paso 6: Copiar los datos de ChatMessages a la tabla temporal usando el mapeo de IDs
INSERT INTO #TempChatMessages (Id, ConversationId, SenderId, Content, SentAt, IsRead)
SELECT cm.Id, m.NewId, cm.SenderId, cm.Content, cm.SentAt, cm.IsRead
FROM ChatMessages cm
JOIN #IdMapping m ON cm.ConversationId = m.OldId;

-- Paso 7: Eliminar las tablas originales (primero la tabla dependiente)
-- Desactivar temporalmente las restricciones de clave externa para poder eliminar las tablas
ALTER TABLE ChatMessages DROP CONSTRAINT FK_ChatMessages_Conversations_ConversationId;

-- Eliminar las tablas originales
DROP TABLE ChatMessages;
DROP TABLE Conversations;

-- Paso 8: Crear las nuevas tablas con la estructura correcta
-- Crear tabla Conversations con Id INT IDENTITY
CREATE TABLE Conversations (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId NVARCHAR(450) NOT NULL,
    AdminId NVARCHAR(450) NULL,
    IsAdminBroadcast BIT NOT NULL DEFAULT 0,
    IsClosed BIT NOT NULL DEFAULT 0,
    IsReadByAdmin BIT NOT NULL DEFAULT 0,
    IsReadByUser BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    LastMessageAt DATETIME2 NULL,
    CONSTRAINT FK_Conversations_AspNetUsers_UserId FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Conversations_AspNetUsers_AdminId FOREIGN KEY (AdminId) REFERENCES AspNetUsers(Id)
);

-- Crear tabla ChatMessages con ConversationId INT
CREATE TABLE ChatMessages (
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ConversationId INT NOT NULL,
    SenderId NVARCHAR(450) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    SentAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsRead BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_ChatMessages_Conversations_ConversationId FOREIGN KEY (ConversationId) REFERENCES Conversations(Id) ON DELETE CASCADE
);

-- Paso 9: Restaurar los datos desde las tablas temporales
-- Establecer el valor de IDENTITY_INSERT para poder insertar IDs específicos
SET IDENTITY_INSERT Conversations ON;

-- Insertar los datos de Conversations
INSERT INTO Conversations (Id, UserId, AdminId, IsAdminBroadcast, IsClosed, IsReadByAdmin, IsReadByUser, CreatedAt, LastMessageAt)
SELECT Id, UserId, AdminId, IsAdminBroadcast, IsClosed, IsReadByAdmin, IsReadByUser, CreatedAt, LastMessageAt
FROM #TempConversations;

SET IDENTITY_INSERT Conversations OFF;

-- Establecer el valor de IDENTITY_INSERT para ChatMessages
SET IDENTITY_INSERT ChatMessages ON;

-- Insertar los datos de ChatMessages
INSERT INTO ChatMessages (Id, ConversationId, SenderId, Content, SentAt, IsRead)
SELECT Id, ConversationId, SenderId, Content, SentAt, IsRead
FROM #TempChatMessages;

SET IDENTITY_INSERT ChatMessages OFF;

-- Paso 10: Limpiar las tablas temporales
DROP TABLE #TempChatMessages;
DROP TABLE #TempConversations;
DROP TABLE #IdMapping;

-- Paso 11: Crear índices para mejorar el rendimiento
CREATE INDEX IX_ChatMessages_ConversationId ON ChatMessages(ConversationId);
CREATE INDEX IX_Conversations_UserId ON Conversations(UserId);
CREATE INDEX IX_Conversations_AdminId ON Conversations(AdminId);

-- Confirmar la transacción si todo salió bien
COMMIT TRANSACTION;

-- Si por alguna razón hay un error, se puede usar:
-- ROLLBACK TRANSACTION;
