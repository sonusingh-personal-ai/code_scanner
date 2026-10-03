USE [BarCodeSannerV3]
GO
/****** Object:  StoredProcedure [dbo].[usp_Line_DELETE] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER PROCEDURE [dbo].[usp_Line_DELETE]
	@Id INT = NULL
AS
BEGIN
	IF (@Id IS NOT NULL AND @Id > 0)
	BEGIN
		UPDATE [dbo].[Response]
		SET [Line] = NULL
		WHERE [Line] = @Id;

		DELETE FROM [dbo].[Line]
		WHERE [Id] = @Id;
	END
	ELSE
	BEGIN
		UPDATE [dbo].[Response]
		SET [Line] = NULL;

		DELETE FROM [dbo].[Line];
	END
END
GO
