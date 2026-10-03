USE [BarCodeSannerV3]
GO
/****** Object:  StoredProcedure [dbo].[usp_Line_READ] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
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
