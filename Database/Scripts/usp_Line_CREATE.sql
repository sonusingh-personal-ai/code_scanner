USE [BarCodeSannerV3]
GO
/****** Object:  StoredProcedure [dbo].[usp_Line_CREATE] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
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
	VALUES (@Name, @Sequence, ISNULL(@CreatedOn, GETDATE()))

	SELECT SCOPE_IDENTITY() AS Id
END
GO
