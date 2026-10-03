USE [BarCodeSannerV3]
GO
/****** Object:  StoredProcedure [dbo].[usp_Line_UPDATE] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER PROCEDURE [dbo].[usp_Line_UPDATE]
	@Id INT,
	@Name NVARCHAR(100),
	@ModifiedOn DATETIME = NULL
AS
BEGIN
	UPDATE [dbo].[Line]
	SET [Name] = @Name,
		[ModifiedOn] = ISNULL(@ModifiedOn, GETDATE())
	WHERE [Id] = @Id
END
GO
