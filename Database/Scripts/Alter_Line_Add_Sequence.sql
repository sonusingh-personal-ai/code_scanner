USE [BarCodeSannerV3];
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- 1. Add Sequence column to Line table if it does not exist
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[Line]') 
    AND name = 'Sequence'
)
BEGIN
    ALTER TABLE [dbo].[Line]
    ADD [Sequence] INT NOT NULL CONSTRAINT [DF_Line_Sequence] DEFAULT (0);
END
GO

-- 2. Populate existing lines with sequential order if Sequence is 0
UPDATE [dbo].[Line]
SET [Sequence] = [Id]
WHERE [Sequence] = 0 OR [Sequence] IS NULL;
GO

-- 3. Update usp_Line_CREATE to accept @Sequence
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

-- 4. Update usp_Line_READ to order by Sequence ASC, Id ASC
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

-- 5. Update usp_Line_UPDATE to update @Sequence if provided
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
