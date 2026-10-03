USE [BarCodeSannerV3];
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- 1. Create Line table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Line]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Line] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Sequence] INT NOT NULL CONSTRAINT [DF_Line_Sequence] DEFAULT (0),
        [CreatedOn] DATETIME NOT NULL CONSTRAINT [DF_Line_CreatedOn] DEFAULT (GETDATE()),
        [ModifiedOn] DATETIME NULL,
        CONSTRAINT [PK_Line] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
ELSE IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Line]') 
    AND name = 'Sequence'
)
BEGIN
    ALTER TABLE [dbo].[Line]
    ADD [Sequence] INT NOT NULL CONSTRAINT [DF_Line_Sequence] DEFAULT (0);
END
GO

-- 2. Seed default lines (Line 1 to Line 4) if table is empty
IF NOT EXISTS (SELECT 1 FROM [dbo].[Line])
BEGIN
    SET IDENTITY_INSERT [dbo].[Line] ON;
    INSERT INTO [dbo].[Line] ([Id], [Name], [Sequence], [CreatedOn]) VALUES
        (1, 'Line 1', 1, GETDATE()),
        (2, 'Line 2', 2, GETDATE()),
        (3, 'Line 3', 3, GETDATE()),
        (4, 'Line 4', 4, GETDATE());
    SET IDENTITY_INSERT [dbo].[Line] OFF;
END
ELSE
BEGIN
    UPDATE [dbo].[Line]
    SET [Sequence] = [Id]
    WHERE [Sequence] = 0 OR [Sequence] IS NULL;
END
GO

-- 3. Stored Procedure: usp_Line_CREATE
CREATE OR ALTER PROCEDURE [dbo].[usp_Line_CREATE]
    @Name NVARCHAR(100),
    @Sequence INT = NULL,
    @CreatedOn DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF (@Sequence IS NULL OR @Sequence <= 0)
    BEGIN
        SELECT @Sequence = ISNULL(MAX([Sequence]), 0) + 1 FROM [dbo].[Line];
    END

    INSERT INTO [dbo].[Line] ([Name], [Sequence], [CreatedOn])
    VALUES (@Name, @Sequence, ISNULL(@CreatedOn, GETDATE()));

    SELECT SCOPE_IDENTITY() AS Id;
END
GO

-- 4. Stored Procedure: usp_Line_READ
CREATE OR ALTER PROCEDURE [dbo].[usp_Line_READ]
    @Id INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF (@Id IS NOT NULL AND @Id > 0)
        SELECT * FROM [dbo].[Line] WHERE [Id] = @Id;
    ELSE
        SELECT * FROM [dbo].[Line] ORDER BY [Sequence] ASC, [Id] ASC;
END
GO

-- 5. Stored Procedure: usp_Line_UPDATE
CREATE OR ALTER PROCEDURE [dbo].[usp_Line_UPDATE]
    @Id INT,
    @Name NVARCHAR(100),
    @Sequence INT = NULL,
    @ModifiedOn DATETIME = NULL
AS
BEGIN
    UPDATE [dbo].[Line]
    SET [Name] = @Name,
        [Sequence] = ISNULL(@Sequence, [Sequence]),
        [ModifiedOn] = ISNULL(@ModifiedOn, GETDATE())
    WHERE [Id] = @Id;
END
GO

-- 6. Stored Procedure: usp_Line_DELETE
CREATE OR ALTER PROCEDURE [dbo].[usp_Line_DELETE]
    @Id INT = NULL
AS
BEGIN
    IF (@Id IS NOT NULL AND @Id > 0)
    BEGIN
        -- Reset Response.Line referencing this line ID
        UPDATE [dbo].[Response]
        SET [Line] = NULL
        WHERE [Line] = @Id;

        DELETE FROM [dbo].[Line]
        WHERE [Id] = @Id;
    END
    ELSE
    BEGIN
        -- Reset all Line references in Response
        UPDATE [dbo].[Response]
        SET [Line] = NULL;

        DELETE FROM [dbo].[Line];
    END
END
GO

-- 7. Stored Procedure: usp_Response_ResetLine
CREATE OR ALTER PROCEDURE [dbo].[usp_Response_ResetLine]
    @LineId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF (@LineId IS NOT NULL AND @LineId > 0)
    BEGIN
        UPDATE [dbo].[Response]
        SET [Line] = NULL
        WHERE [Line] = @LineId;
    END
    ELSE
    BEGIN
        UPDATE [dbo].[Response]
        SET [Line] = NULL;
    END
END
GO
