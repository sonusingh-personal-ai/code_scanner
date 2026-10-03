USE [BarCodeSannerV3]
GO
/****** Object:  StoredProcedure [dbo].[usp_Line_CREATE] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER PROCEDURE [dbo].[usp_Line_CREATE]
	@Name NVARCHAR(100),
	@CreatedOn DATETIME = NULL
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO [dbo].[Line] ([Name], [CreatedOn])
	VALUES (@Name, ISNULL(@CreatedOn, GETDATE()))

	SELECT SCOPE_IDENTITY() AS Id
END
GO
