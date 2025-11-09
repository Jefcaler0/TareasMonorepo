





IF DB_ID(N'Tareas') IS NULL
BEGIN
    EXEC('CREATE DATABASE Tareas;');
END
GO

USE Tareas;
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = N'todo')
BEGIN
    EXEC('CREATE SCHEMA todo AUTHORIZATION dbo;');
END
GO

IF OBJECT_ID(N'todo.Tasks', N'U') IS NULL
BEGIN
    CREATE TABLE todo.Tasks
    (
        Id INT IDENTITY(1, 1) PRIMARY KEY,
        Title NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NULL,s
        Status TINYINT NOT NULL CONSTRAINT DF_Tasks_Status DEFAULT (0),
        CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Tasks_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt DATETIME2 NOT NULL CONSTRAINT DF_Tasks_UpdatedAt DEFAULT (SYSUTCDATETIME())
    );
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Tasks_Status'
      AND object_id = OBJECT_ID(N'todo.Tasks', N'U')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Tasks_Status
        ON todo.Tasks (Status)
        INCLUDE (CreatedAt, Id);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Tasks_CreatedAt'
      AND object_id = OBJECT_ID(N'todo.Tasks', N'U')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Tasks_CreatedAt
        ON todo.Tasks (CreatedAt DESC, Id DESC);
END
GO

IF TYPE_ID(N'todo.TaskStatus') IS NULL
BEGIN
    EXEC('CREATE TYPE todo.TaskStatus FROM TINYINT NOT NULL;');
END
GO

CREATE OR ALTER PROCEDURE todo.spTasks_GetAll
AS
BEGIN
    --SET NOCOUNT ON;

    SELECT
        Id,
        Title,
        Description,
        Status,
        CreatedAt,
        UpdatedAt
    FROM todo.Tasks
    ORDER BY CreatedAt DESC, Id DESC;
END
GO

CREATE OR ALTER PROCEDURE todo.spTasks_GetById
    @Id INT
AS
BEGIN
   -- SET NOCOUNT ON;

    SELECT
        Id,
        Title,
        Description,
        Status,
        CreatedAt,
        UpdatedAt
    FROM todo.Tasks
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE todo.spTasks_Insert
    @Title NVARCHAR(100),
    @Description NVARCHAR(500) = NULL,
    @Status todo.TaskStatus = 0,
    @CreatedAt DATETIME2,
    @UpdatedAt DATETIME2,
    @NewId INT OUTPUT
AS
BEGIN
   -- SET NOCOUNT ON;

    INSERT INTO todo.Tasks (Title, Description, Status, CreatedAt, UpdatedAt)
    VALUES (@Title, @Description, @Status, @CreatedAt, @UpdatedAt);

    SET @NewId = SCOPE_IDENTITY();

    SELECT
        Id,
        Title,
        Description,
        Status,
        CreatedAt,
        UpdatedAt
    FROM todo.Tasks
    WHERE Id = @NewId;
END
GO

CREATE OR ALTER PROCEDURE todo.spTasks_Update
    @Id INT,
    @Title NVARCHAR(100),
    @Description NVARCHAR(500) = NULL,
    @Status todo.TaskStatus,
    @UpdatedAt DATETIME2
AS
BEGIN
    UPDATE todo.Tasks
    SET
        Title = @Title,
        Description = @Description,
        Status = @Status,
        UpdatedAt = @UpdatedAt
    WHERE Id = @Id;

    SELECT
        Id,
        Title,
        Description,
        Status,
        CreatedAt,
        UpdatedAt
    FROM todo.Tasks
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE todo.spTasks_Delete
    @Id INT
AS
BEGIN
    DELETE FROM todo.Tasks
    WHERE Id = @Id;
END
GO

